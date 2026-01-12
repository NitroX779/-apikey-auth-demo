const sqlite3 = require('sqlite3').verbose();
const bcrypt = require('bcrypt');
const { v4: uuidv4 } = require('uuid');
const path = require('path');

const dbPath = path.join(__dirname, 'data.sqlite');
const db = new sqlite3.Database(dbPath);

function init() {
  db.serialize(() => {
    db.run(`
      CREATE TABLE IF NOT EXISTS users (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        username TEXT UNIQUE,
        password_hash TEXT
      );
    `);

    db.run(`
      CREATE TABLE IF NOT EXISTS api_keys (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        user_id INTEGER,
        key TEXT UNIQUE,
        label TEXT,
        created_at TEXT,
        expiry_days INTEGER DEFAULT 30,
        max_uses INTEGER DEFAULT 1,
        used_count INTEGER DEFAULT 0,
        banned INTEGER DEFAULT 0,
        format TEXT DEFAULT 'Contental_X-',
        hwid TEXT,
        ip TEXT
      );
    `);

    db.run(`ALTER TABLE api_keys ADD COLUMN expiry_days INTEGER DEFAULT 30`, (err) => { if (err && !err.message.includes('duplicate column')) console.error(err); });
    db.run(`ALTER TABLE api_keys ADD COLUMN max_uses INTEGER DEFAULT 1`, (err) => { if (err && !err.message.includes('duplicate column')) console.error(err); });
    db.run(`ALTER TABLE api_keys ADD COLUMN used_count INTEGER DEFAULT 0`, (err) => { if (err && !err.message.includes('duplicate column')) console.error(err); });
    db.run(`ALTER TABLE api_keys ADD COLUMN banned INTEGER DEFAULT 0`, (err) => { if (err && !err.message.includes('duplicate column')) console.error(err); });
    db.run(`ALTER TABLE api_keys ADD COLUMN format TEXT DEFAULT 'Contental_X-'`, (err) => { if (err && !err.message.includes('duplicate column')) console.error(err); });
    db.run(`ALTER TABLE api_keys ADD COLUMN hwid TEXT`, (err) => { if (err && !err.message.includes('duplicate column')) console.error(err); });
    db.run(`ALTER TABLE api_keys ADD COLUMN ip TEXT`, (err) => { if (err && !err.message.includes('duplicate column')) console.error(err); });
  });
}

function createUser(username, password) {
  return new Promise(async (resolve, reject) => {
    const hash = await bcrypt.hash(password, 10);
    db.run('INSERT INTO users (username, password_hash) VALUES (?, ?)', [username, hash], function(err) {
      if (err) reject(err);
      else resolve(this.lastID);
    });
  });
}

function getUserByUsername(username) {
  return new Promise((resolve, reject) => {
    db.get('SELECT * FROM users WHERE username = ?', [username], (err, row) => {
      if (err) reject(err);
      else resolve(row);
    });
  });
}

function verifyUser(username, password) {
  return new Promise(async (resolve, reject) => {
    try {
      const user = await getUserByUsername(username);
      if (!user) return resolve(null);
      const ok = await bcrypt.compare(password, user.password_hash);
      resolve(ok ? user : null);
    } catch (err) {
      reject(err);
    }
  });
}

function ensureAdminUser() {
  getUserByUsername('admin').then(user => {
    if (!user) {
      bcrypt.hash('admin', 10).then(hash => {
        db.run('INSERT INTO users (username, password_hash) VALUES (?, ?)', ['admin', hash], (err) => {
          if (!err) console.log('Created default user admin / admin (change password!).');
        });
      });
    }
  }).catch(console.error);
}

function createApiKey(userId, label = '', expiryDays = 30, maxUses = 1, format = 'ContentalX-', customSuffix = '') {
  return new Promise((resolve, reject) => {
    let suffix = customSuffix;
    if (!suffix) {
      suffix = uuidv4().substring(0, 8);
    }
    const token = format + suffix;
    db.run('INSERT INTO api_keys (user_id, key, label, created_at, expiry_days, max_uses, format) VALUES (?, ?, ?, ?, ?, ?, ?)', [userId, token, label, new Date().toISOString(), expiryDays, maxUses, format], function(err) {
      if (err) reject(err);
      else resolve(token);
    });
  });
}

function getApiKeysByUser(userId) {
  return new Promise((resolve, reject) => {
    db.all('SELECT id, key, label, created_at, expiry_days, max_uses, used_count, banned, format, hwid, ip FROM api_keys WHERE user_id = ?', [userId], (err, rows) => {
      if (err) reject(err);
      else resolve(rows);
    });
  });
}

function validateApiKey(key, hwid, ip) {
  return new Promise((resolve, reject) => {
    db.get('SELECT k.id, k.key, k.label, k.created_at, k.expiry_days, k.max_uses, k.used_count, k.banned, k.hwid, k.ip, u.id as user_id, u.username FROM api_keys k JOIN users u ON u.id = k.user_id WHERE k.key = ?', [key], (err, row) => {
      if (err) reject(err);
      else if (!row) resolve(null);
      else {
        const created = new Date(row.created_at);
        const expiry = new Date(created.getTime() + row.expiry_days * 24 * 60 * 60 * 1000);
        const now = new Date();
        if (row.banned || now > expiry || (row.max_uses > 1 && row.used_count >= row.max_uses)) {
          resolve(null);
        } else if (row.hwid && row.hwid !== hwid) {
          resolve(null); // HWID mismatch
        } else {
          // Set HWID and IP if not set
          if (!row.hwid) {
            db.run('UPDATE api_keys SET hwid = ?, ip = ? WHERE id = ?', [hwid, ip, row.id]);
          }
          // Always increment used_count when key is used
          db.run('UPDATE api_keys SET used_count = used_count + 1 WHERE id = ?', [row.id]);
          resolve(row);
        }
      }
    });
  });
}

function deleteApiKey(id, userId) {
  return new Promise((resolve, reject) => {
    db.run('DELETE FROM api_keys WHERE id = ? AND user_id = ?', [id, userId], function(err) {
      if (err) reject(err);
      else resolve(this.changes > 0);
    });
  });
}

function banApiKey(id, userId, banned = 1) {
  return new Promise((resolve, reject) => {
    db.run('UPDATE api_keys SET banned = ? WHERE id = ? AND user_id = ?', [banned, id, userId], function(err) {
      if (err) reject(err);
      else resolve(this.changes > 0);
    });
  });
}

function resetApiKey(id, userId) {
  return new Promise((resolve, reject) => {
    db.run('UPDATE api_keys SET used_count = 0 WHERE id = ? AND user_id = ?', [id, userId], function(err) {
      if (err) reject(err);
      else resolve(this.changes > 0);
    });
  });
}

init();

module.exports = {
  init,
  createUser,
  getUserByUsername,
  verifyUser,
  ensureAdminUser,
  createApiKey,
  getApiKeysByUser,
  validateApiKey,
  deleteApiKey,
  banApiKey,
  resetApiKey
};
