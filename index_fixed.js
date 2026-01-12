const express = require('express');
const bodyParser = require('body-parser');
const session = require('express-session');
const path = require('path');
const cors = require('cors');
const db = require('./db');

const app = express();
const PORT = process.env.PORT || 3000;

app.use(cors());
app.use(bodyParser.urlencoded({ extended: true }));
app.use(bodyParser.json());
app.use(session({ secret: 'dev-secret-change-me', resave: false, saveUninitialized: false }));
app.use(express.static(path.join(__dirname, 'public')));

// ensure admin user exists
db.ensureAdminUser();

function requireLogin(req, res, next) {
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

app.post('/logout', (req, res) => {
  req.session.destroy(() => res.json({ ok: true }));
});

app.get('/me', (req, res) => {
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
  const { label, expiryDays, maxUses, format, customSuffix, count } = req.body;
  const numKeys = count || 1;
  const keys = [];
  for (let i = 0; i < numKeys; i++) {
    try {
      const token = await db.createApiKey(req.session.userId, label || '', expiryDays || 30, maxUses || 1, format || 'Contental_X-', customSuffix || '');
      keys.push(token);
    } catch (err) {
      return res.status(500).json({ error: 'Failed to create key' });
    }
  }
  res.json({ keys });
});

app.post('/api/validate-key', async (req, res) => {
  const key = req.header('x-api-key') || req.body.key;
  if (!key) return res.status(400).json({ error: 'Missing key' });
  try {
    const info = await db.validateApiKey(key);
    if (!info) return res.status(401).json({ error: 'Invalid key' });
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

