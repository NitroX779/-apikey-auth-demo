const express = require('express');
const bodyParser = require('body-parser');
const session = require('express-session');
const path = require('path');
const cors = require('cors');
const axios = require('axios');
const crypto = require('crypto');

// KeyAuth configuration
const KEYAUTH_CONFIG = {
    name: 'Bypass',
    ownerid: 'NYVOikiir1',
    version: '1.0',
    secret: '6895fce8f8dd9622d0bf33222cc7486d71d34902a2d8aa0ecfadc159db2ed60b',
    url: 'https://keyauth.win/api/1.2/'
};

// Generate checksum function
function getChecksum() {
    return require('crypto').createHash('sha256').update('your-app-checksum-data').digest('hex');
}

// Generate checksum (similar to Python getchecksum)
function getChecksum() {
    // This is a simplified version - in production, match Python implementation
    return crypto.createHash('sha256').update('your-app-checksum-data').digest('hex');
}
const apiKeys = [];

const db = {
  init: () => console.log('Demo mode - no database'),
  ensureAdminUser: () => {},
  verifyUser: async (username, password) => {
    console.log('Attempting KeyAuth login for user:', username);
    
    // Block admin/admin explicitly
    if (username === 'admin' && password === 'admin') {
      console.log('Blocked admin/admin login attempt');
      return Promise.resolve(null);
    }
    
    try {
      // First initialize session with KeyAuth
      const initData = {
        type: 'init',
        ownerid: KEYAUTH_CONFIG.ownerid,
        name: KEYAUTH_CONFIG.name,
        ver: KEYAUTH_CONFIG.version,
        secret: KEYAUTH_CONFIG.secret
      };
      
      console.log('Sending init request to KeyAuth...', initData);
      const initQueryString = Object.keys(initData)
        .map(key => `${encodeURIComponent(key)}=${encodeURIComponent(initData[key])}`)
        .join('&');
      
      const initUrl = `${KEYAUTH_CONFIG.url}?${initQueryString}`;
      console.log('Init API URL:', initUrl);
      
      const initResponse = await axios.get(initUrl);
      console.log('Init response:', initResponse.data);
      
      if (initResponse.data.success) {
        // Now attempt login with session ID
        const loginData = {
          type: 'login',
          username: username,
          pass: password,
          sessionid: initResponse.data.sessionid,  // Using session ID as key
          name: KEYAUTH_CONFIG.name,
          ownerid: KEYAUTH_CONFIG.ownerid
        };
        
        console.log('Sending login request to KeyAuth...', loginData);
        const loginQueryString = Object.keys(loginData)
          .map(key => `${encodeURIComponent(key)}=${encodeURIComponent(loginData[key])}`)
          .join('&');
        
        const loginUrl = `${KEYAUTH_CONFIG.url}?${loginQueryString}`;
        console.log('Login API URL:', loginUrl);
        
        const loginResponse = await axios.get(loginUrl);
        console.log('Login response:', loginResponse.data);
        
        if (loginResponse.data.success) {
          console.log('KeyAuth login successful for:', username);
          return Promise.resolve({
            id: loginResponse.data.userid || 1,
            username: username,
            keyauth_data: loginResponse.data
          });
        } else {
          console.log('KeyAuth login failed:', loginResponse.data.message);
        }
      } else {
        console.log('KeyAuth init failed:', initResponse.data.message);
      }
      
      return Promise.resolve(null);
    } catch (error) {
      console.error('KeyAuth authentication error:', error.message);
      if (error.response) {
        console.error('Response data:', error.response.data);
      }
      return Promise.resolve(null);
    }
  },
  getApiKeysByUser: (userId) => Promise.resolve(apiKeys),
  createApiKey: (userId, label, expiryDays, maxUses, format, customSuffix) => {
    const key = format + (customSuffix || Math.random().toString(36).substring(2, 10));
    apiKeys.push({
      id: apiKeys.length + 1,
      key,
      label: label || '',
      created_at: new Date().toISOString(),
      expiry_days: expiryDays || 30,
      max_uses: maxUses || 1,
      used_count: 0,
      banned: 0,
      format: format || 'ContentalX-',
      hwid: null,
      ip: null
    });
    return Promise.resolve(key);
  },
  validateApiKey: (key, hwid, ip) => {
    const foundKey = apiKeys.find(k => k.key === key);
    if (foundKey && !foundKey.banned) {
      foundKey.used_count++;
      foundKey.hwid = foundKey.hwid || hwid;
      foundKey.ip = foundKey.ip || ip;
      return Promise.resolve({
        ...foundKey,
        user_id: 1,
        username: 'admin'
      });
    }
    return Promise.resolve(null);
  },
  deleteApiKey: (id, userId) => {
    const index = apiKeys.findIndex(k => k.id == id);
    if (index !== -1) {
      apiKeys.splice(index, 1);
      return Promise.resolve(true);
    }
    return Promise.resolve(false);
  },
  banApiKey: (id, userId, banned) => {
    const key = apiKeys.find(k => k.id == id);
    if (key) {
      key.banned = banned ? 1 : 0;
      return Promise.resolve(true);
    }
    return Promise.resolve(false);
  },
  resetApiKey: (id, userId) => {
    const key = apiKeys.find(k => k.id == id);
    if (key) {
      key.used_count = 0;
      return Promise.resolve(true);
    }
    return Promise.resolve(false);
  }
};

const app = express();
const PORT = process.env.PORT || 3000;

console.log('Starting server with KeyAuth integration...');

app.use(cors());
app.use(bodyParser.urlencoded({ extended: true }));
app.use(bodyParser.json());
app.use(session({ secret: 'dev-secret-change-me', resave: false, saveUninitialized: false, cookie: { httpOnly: false, secure: false } }));
app.use(express.static(path.join(__dirname, 'public')));
app.use('/Logo', express.static(path.join(__dirname, 'Logo')));

// Serve login.html for root path
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'login.html'));
});

// ensure admin user exists
db.init();
db.ensureAdminUser();

function requireLogin(req, res, next) {
  console.log('requireLogin, session:', req.session ? req.session.userId : 'no session');
  if (req.session && req.session.userId) return next();
  return res.status(401).json({ error: 'Not authenticated' });
}

app.post('/login', async (req, res) => {
  const { username, password } = req.body;
  try {
    const user = await db.verifyUser(username, password);
    if (!user) return res.status(401).json({ error: 'Invalid credentials' });
    req.session.userId = user.id;
    req.session.username = user.username;
    res.json({ ok: true, username: user.username });
  } catch (err) {
    res.status(500).json({ error: 'Server error' });
  }
});

app.post('/register', async (req, res) => {
  const { username, password, key } = req.body;
  if (!username || !password || !key) {
    return res.status(400).json({ error: 'Missing fields' });
  }
  try {
    const initData = {
      type: 'init',
      ownerid: KEYAUTH_CONFIG.ownerid,
      name: KEYAUTH_CONFIG.name,
      ver: KEYAUTH_CONFIG.version,
      secret: KEYAUTH_CONFIG.secret
    };
    const initQueryString = Object.keys(initData)
      .map(key => `${encodeURIComponent(key)}=${encodeURIComponent(initData[key])}`)
      .join('&');
    const initUrl = `${KEYAUTH_CONFIG.url}?${initQueryString}`;
    const initResponse = await axios.get(initUrl);

    if (!initResponse.data.success) {
      return res.status(400).json({ error: initResponse.data.message || 'Init failed' });
    }

    const registerData = {
      type: 'register',
      username: username,
      pass: password,
      key: key,
      sessionid: initResponse.data.sessionid,
      name: KEYAUTH_CONFIG.name,
      ownerid: KEYAUTH_CONFIG.ownerid
    };
    const registerQueryString = Object.keys(registerData)
      .map(k => `${encodeURIComponent(k)}=${encodeURIComponent(registerData[k])}`)
      .join('&');
    const registerUrl = `${KEYAUTH_CONFIG.url}?${registerQueryString}`;
    const registerResponse = await axios.get(registerUrl);

    if (!registerResponse.data.success) {
      return res.status(400).json({ error: registerResponse.data.message || 'Registration failed' });
    }

    const user = await db.verifyUser(username, password);
    if (!user) return res.json({ ok: true, registered: true });
    req.session.userId = user.id;
    req.session.username = user.username;
    res.json({ ok: true, username: user.username });
  } catch (err) {
    res.status(500).json({ error: 'Server error' });
  }
});

app.post('/logout', (req, res) => {
  console.log('Logging out user:', req.session ? req.session.userId : 'no session');
  req.session.destroy((err) => {
    if (err) console.error('Error destroying session:', err);
    res.json({ ok: true });
  });
});

app.get('/me', (req, res) => {
  console.log('Checking /me:', req.session ? req.session.userId : 'no session');
  if (!req.session || !req.session.userId) return res.json({ loggedIn: false });
  res.json({ loggedIn: true, username: req.session.username });
});

app.get('/api/keys', requireLogin, async (req, res) => {
  try {
    const keys = await db.getApiKeysByUser(req.session.userId);
    res.json(keys);
  } catch (err) {
    res.status(500).json({ error: 'Server error' });
  }
});

app.post('/api/keys', requireLogin, async (req, res) => {
  console.log('Creating keys for user:', req.session.userId);
  const { expiryDays, maxUses, count, customPart } = req.body;
  const numKeys = count || 1;
  const keys = [];
  for (let i = 0; i < numKeys; i++) {
    try {
      const token = await db.createApiKey(req.session.userId, '', expiryDays || 30, maxUses || 1, 'ContentalX-', customPart || '');
      keys.push(token);
    } catch (err) {
      console.error('Error creating key:', err);
      return res.status(500).json({ error: 'Failed to create key' });
    }
  }
  console.log('Created keys:', keys);
  res.json({ keys });
});

app.post('/api/validate-key', async (req, res) => {
  const key = req.header('x-api-key') || req.body.key;
  const hwid = req.header('x-hwid') || req.body.hwid;
  const ip = req.headers['x-forwarded-for'] || req.connection.remoteAddress;
  
  if (!key) return res.status(400).json({ error: 'Missing key' });
  if (!hwid) return res.status(400).json({ error: 'Missing HWID' });
  
  try {
    const info = await db.validateApiKey(key, hwid, ip);
    if (!info) return res.status(401).json({ error: 'Invalid key or HWID' });
    res.json({ ok: true, user: { id: info.user_id, username: info.username }, key: { id: info.id, label: info.label, created_at: info.created_at } });
  } catch (err) {
    res.status(500).json({ error: 'Server error' });
  }
});

app.delete('/api/keys/:id', requireLogin, async (req, res) => {
  const id = req.params.id;
  try {
    const success = await db.deleteApiKey(id, req.session.userId);
    res.json({ success });
  } catch (err) {
    res.status(500).json({ error: 'Server error' });
  }
});

app.patch('/api/keys/:id/ban', requireLogin, async (req, res) => {
  const id = req.params.id;
  const banned = req.body.banned ? 1 : 0;
  try {
    const success = await db.banApiKey(id, req.session.userId, banned);
    res.json({ success });
  } catch (err) {
    res.status(500).json({ error: 'Server error' });
  }
});

app.patch('/api/keys/:id/reset', requireLogin, async (req, res) => {
  const id = req.params.id;
  try {
    const success = await db.resetApiKey(id, req.session.userId);
    res.json({ success });
  } catch (err) {
    res.status(500).json({ error: 'Server error' });
  }
});

app.patch('/api/keys/:id', requireLogin, async (req, res) => {
  const id = req.params.id;
  const updates = req.body;
  try {
    const success = await db.updateApiKey(id, req.session.userId, updates);
    res.json({ success });
  } catch (err) {
    res.status(500).json({ error: 'Server error' });
  }
});

app.listen(PORT, () => {
  console.log(`Server running on port ${PORT}`);
});
