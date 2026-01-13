const express = require('express');
const axios = require('axios');
const bodyParser = require('body-parser');
const session = require('express-session');
const path = require('path');
const cors = require('cors');
const db = require('./db');

const app = express();
const PORT = process.env.PORT || 3000;

// KeyAuth configuration
const KEYAUTH_CONFIG = {
    name: 'Bypass',
    ownerid: 'NYVOikiir1',
    version: '1.0',
    secret: '6895fce8f8dd9622d0bf33222cc7486d71d34902a2d8aa0ecfadc159db2ed60b',
    url: 'https://keyauth.win/api/1.2/'
};

console.log('KeyAuth Configuration Loaded:', {
    name: KEYAUTH_CONFIG.name,
    ownerid: KEYAUTH_CONFIG.ownerid,
    version: KEYAUTH_CONFIG.version,
    url: KEYAUTH_CONFIG.url,
    secret_length: KEYAUTH_CONFIG.secret.length
});

app.use(cors());
app.use(bodyParser.urlencoded({ extended: true }));
app.use(bodyParser.json());
app.use(session({ 
  secret: 'dev-secret-change-me', 
  resave: false, 
  saveUninitialized: false, 
  cookie: { 
    httpOnly: false, 
    secure: false,
    maxAge: 24 * 60 * 60 * 1000 // 24 hours
  },
  rolling: true // Reset timeout on each request
}));
app.use(express.static(path.join(__dirname, 'public')));
app.use('/Logo', express.static(path.join(__dirname, 'Logo')));

// Serve login.html for root path
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'login.html'));
});

// Initialize database
db.init();

// Note: ensureAdminUser() is commented out in db.js
// Uncomment in db.js and here to restore local admin user

// KeyAuth utility functions
async function keyAuthLogin(username, password) {
  try {
    console.log('Attempting KeyAuth login for user:', username);
    
    // First initialize session with KeyAuth (like index.txt)
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
      // Now attempt login with session ID (like index.txt)
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
        return { success: true, message: 'Login successful' };
      } else {
        console.log('KeyAuth login failed:', loginResponse.data.message);
        return { success: false, message: loginResponse.data.message || 'Invalid credentials' };
      }
    } else {
      console.log('KeyAuth init failed:', initResponse.data.message);
      return { success: false, message: initResponse.data.message || 'Authentication service unavailable' };
    }
    
  } catch (error) {
    console.error('KeyAuth authentication error:', error.message);
    if (error.response) {
      console.error('Response data:', error.response.data);
    }
    return { success: false, message: 'Authentication service unavailable: ' + error.message };
  }
}

async function keyAuthRegister(username, password, license) {
  try {
    // First initialize session with KeyAuth (like index.txt)
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
      return { success: false, message: initResponse.data.message || 'Init failed' };
    }
    
    // Now attempt registration with session ID (like index.txt)
    const registerData = {
      type: 'register',
      username: username,
      pass: password,
      key: license,  // Using the license key as 'key' parameter
      sessionid: initResponse.data.sessionid,
      name: KEYAUTH_CONFIG.name,
      ownerid: KEYAUTH_CONFIG.ownerid
    };
    
    const registerQueryString = Object.keys(registerData)
      .map(k => `${encodeURIComponent(k)}=${encodeURIComponent(registerData[k])}`)
      .join('&');
    
    const registerUrl = `${KEYAUTH_CONFIG.url}?${registerQueryString}`;
    const registerResponse = await axios.get(registerUrl);
    
    if (registerResponse.data.success) {
      return { success: true, message: 'Registration successful' };
    } else {
      return { success: false, message: registerResponse.data.message || 'Registration failed' };
    }
    
  } catch (error) {
    console.error('KeyAuth registration error:', error.response?.data || error.message);
    return { success: false, message: 'Registration service unavailable' };
  }
}

function requireLogin(req, res, next) {
  console.log('requireLogin, session:', req.session ? req.session.userId : 'no session');
  if (req.session && req.session.userId) return next();
  return res.status(401).json({ error: 'Not authenticated' });
}

app.post('/login', async (req, res) => {
  const { username, password } = req.body;
  
  console.log('Login attempt received for user:', username);
  console.log('Request body:', { username, password: password ? '[PROVIDED]' : '[MISSING]' });
  
  // Validate input
  if (!username || !password) {
    console.log('Login failed: missing credentials');
    return res.status(400).json({ error: 'Username and password required' });
  }
  
  try {
    // KEYAUTH INTEGRATION
    // Authenticate with KeyAuth
    console.log('Calling KeyAuth login function...');
    const authResult = await keyAuthLogin(username, password);
    
    console.log('KeyAuth authentication result:', authResult);
    
    if (!authResult.success) {
      console.log('KeyAuth authentication failed for user:', username);
      return res.status(401).json({ error: authResult.message });
    }
    
    console.log('KeyAuth authentication successful, creating local session...');
    
    // Get or create local user record
    let user = await db.getUserByUsername(username);
    if (!user) {
      console.log('Creating new local user record for:', username);
      // Create local user record for session management
      const userId = await db.createUser(username, password);
      user = { id: userId, username: username };
    } else {
      console.log('Using existing local user record for:', username);
    }
    
    // Set session
    req.session.userId = user.id;
    req.session.username = user.username;
    
    console.log(`User ${username} logged in successfully via KeyAuth`);
    res.json({ ok: true, username: user.username });
    
    /*
    // LOCAL DATABASE AUTHENTICATION (fallback)
    // Uncomment if KeyAuth is not working
    
    console.log('Using local database authentication...');
    
    const user = await db.verifyUser(username, password);
    if (!user) {
      console.log('Local authentication failed for user:', username);
      return res.status(401).json({ error: 'Invalid credentials' });
    }
    
    // Set session
    req.session.userId = user.id;
    req.session.username = user.username;
    
    console.log(`User ${username} logged in successfully via local database`);
    res.json({ ok: true, username: user.username });
    */
    
  } catch (err) {
    console.error('Login error:', err);
    res.status(500).json({ error: 'Server error during authentication' });
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
  
  // Use customPart as label, fallback to 'Nuova Licenza'
  const label = customPart || 'Nuova Licenza';
  
  for (let i = 0; i < numKeys; i++) {
    try {
      const token = await db.createApiKey(req.session.userId, label, expiryDays || 30, maxUses || 1, 'ContentalX-', customPart || '');
      keys.push({
        key: token,
        label: label,
        status: 'active',
        uses: `0/${maxUses || 1}`,
        expiry: new Date(Date.now() + (expiryDays || 30) * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
        hwid: 'N/A'
      });
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

// Registration endpoint
app.post('/register', async (req, res) => {
  const { username, password, key: license } = req.body;
  
  // Validate input
  if (!username || !password || !license) {
    return res.status(400).json({ error: 'Username, password, and license key required' });
  }
  
  try {
    // Register with KeyAuth
    const regResult = await keyAuthRegister(username, password, license);
    
    if (!regResult.success) {
      return res.status(400).json({ error: regResult.message });
    }
    
    // Create local user record
    const userId = await db.createUser(username, password);
    
    // Set session
    req.session.userId = userId;
    req.session.username = username;
    
    console.log(`User ${username} registered successfully via KeyAuth`);
    res.json({ ok: true, username: username });
    
  } catch (err) {
    console.error('Registration error:', err);
    if (err.code === 'SQLITE_CONSTRAINT') {
      return res.status(400).json({ error: 'Username already exists' });
    }
    res.status(500).json({ error: 'Server error during registration' });
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