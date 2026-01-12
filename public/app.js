function getCookie(name) {
  const value = `; ${document.cookie}`;
  const parts = value.split(`; ${name}=`);
  if (parts.length === 2) return parts.pop().split(';').shift();
}

async function domReady() {
  console.log('Cookies:', document.cookie);
  const sessionCookie = getCookie('connect.sid');
  console.log('Session cookie:', sessionCookie);
  const userSubtitle = document.getElementById('userSubtitle');

  const me = await (await fetch('/me', { headers: { 'Cookie': `connect.sid=${getCookie('connect.sid')}` } })).json();
  if (!me.loggedIn) {
    window.location.href = '/login.html';
    return;
  }
  userSubtitle.textContent = 'ContentalX · ' + me.username;

  const createBtn = document.getElementById('create');
  const logoutBtn = document.getElementById('logout');
  const keysList = document.getElementById('keys');
  const notifications = document.getElementById('notifications');
  const preview = document.getElementById('preview');
  const customPart = document.getElementById('customPart');

  function updatePreview() {
    const custom = customPart.value || '';
    preview.textContent = 'ContentalX-' + custom;
  }

  customPart.addEventListener('input', updatePreview);
  updatePreview();

  function showNotification(message, type = 'success') {
    const notif = document.createElement('div');
    notif.className = `notification ${type}`;
    notif.innerHTML = `
      <div class="icon">✓</div>
      <div class="message">${message}</div>
      <button class="copy-btn" onclick="navigator.clipboard.writeText('${message}')">Copiar</button>
    `;
    notifications.appendChild(notif);
    setTimeout(() => notif.remove(), 5000);
  }

  async function loadKeys() {
    const res = await fetch('/api/keys', { headers: { 'Cookie': `connect.sid=${getCookie('connect.sid')}` } });
    const keys = await res.json();
    keysList.innerHTML = '';
    keys.forEach((k, index) => {
      const card = document.createElement('div');
      card.className = 'license-card';
      card.style.animationDelay = `${index * 0.1}s`;

      const now = new Date();
      const expiry = new Date(k.created_at);
      expiry.setDate(expiry.getDate() + k.expiry_days);
      let status = 'active';
      let statusText = 'Activa';
      if (k.banned) {
        status = 'banned';
        statusText = 'Baneada';
      } else if (now > expiry) {
        status = 'expired';
        statusText = 'Expirada';
      }

      card.innerHTML = `
        <div class="key">${k.key}</div>
        <div class="details">
          <span class="status-badge status-${status}">${statusText}</span>
          <span class="used-info">${k.used_count > 0 ? 'Usada' : 'No usada'}</span>
          <input type="checkbox" class="key-checkbox" value="${k.id}" style="margin-right: 10px;">
          <button class="settings-btn" onclick="showSettings(${JSON.stringify(k).replace(/"/g, '&quot;')}, this)">⚙️</button>
        </div>
        <div class="info">
          Label: ${k.label}<br>
          Expira: ${k.expiry_days} días<br>
          Usos: ${k.used_count}/${k.max_uses}<br>
          ${k.hwid || k.ip ? `<div style="font-size: 12px; color: blue; margin-top: 5px;">${k.hwid ? `HWID: ${k.hwid}` : ''}${k.hwid && k.ip ? '<br>' : ''}${k.ip ? `IP: ${k.ip}` : ''}</div>` : ''}
        </div>
      `;
      keysList.appendChild(card);
    });
  }

  createBtn.addEventListener('click', async () => {
    createBtn.style.transform = 'scale(0.95)';
    setTimeout(() => createBtn.style.transform = 'scale(1)', 100);
    console.log('Creating keys, cookies:', document.cookie);
    const expiryDays = parseInt(document.getElementById('expiryDays').value) || 30;
    const maxUses = parseInt(document.getElementById('maxUses').value) || 1;
    const count = parseInt(document.getElementById('count').value) || 1;
    const customPart = document.getElementById('customPart').value;
    const res = await fetch('/api/keys', { method: 'POST', headers: { 'Content-Type': 'application/json', 'Cookie': `connect.sid=${getCookie('connect.sid')}` }, body: JSON.stringify({ expiryDays, maxUses, count, customPart }) });
    console.log('Response status:', res.status);
    if (!res.ok) return showNotification('Error al crear licencias', 'error');
    const body = await res.json();
    showNotification(`Licencias creadas: ${body.keys.join(', ')}`);
    loadKeys();
  });

  logoutBtn.addEventListener('click', async () => {
    console.log('Logging out, cookies:', document.cookie);
    await fetch('/logout', { method: 'POST', headers: { 'Cookie': `connect.sid=${getCookie('connect.sid')}` } });
    window.location.href = '/login.html';
  });

  // Bulk actions
  document.getElementById('deleteSelected').addEventListener('click', async () => {
    const selected = Array.from(document.querySelectorAll('.key-checkbox:checked')).map(cb => cb.value);
    if (selected.length === 0) return alert('Seleziona almeno una chiave');
    if (confirm(`Eliminare ${selected.length} chiavi?`)) {
      for (const id of selected) {
        await fetch(`/api/keys/${id}`, { method: 'DELETE', headers: { 'Cookie': `connect.sid=${getCookie('connect.sid')}` } });
      }
      loadKeys();
    }
  });

  document.getElementById('resetSelected').addEventListener('click', async () => {
    const selected = Array.from(document.querySelectorAll('.key-checkbox:checked')).map(cb => cb.value);
    if (selected.length === 0) return alert('Seleziona almeno una chiave');
    for (const id of selected) {
      await fetch(`/api/keys/${id}/reset`, { method: 'PATCH', headers: { 'Cookie': `connect.sid=${getCookie('connect.sid')}` } });
    }
    loadKeys();
  });

  // Remove old bulk select buttons as per new design

  let currentKey = null;
  let currentDropdown = null;

  function showSettings(key, button) {
    // Close any open dropdown
    if (currentDropdown) {
      currentDropdown.remove();
      currentDropdown = null;
    }

    currentKey = key;

    // Create dropdown
    const dropdown = document.getElementById('dropdownTemplate').cloneNode(true);
    dropdown.id = '';
    dropdown.style.display = 'block';
    dropdown.classList.add('show');

    // Position next to the button
    const rect = button.getBoundingClientRect();
    dropdown.style.left = rect.right + 10 + 'px';
    dropdown.style.top = rect.top + 'px';

    document.body.appendChild(dropdown);
    currentDropdown = dropdown;

    // Close on click outside
    setTimeout(() => {
      document.addEventListener('click', closeDropdownOnClickOutside);
    }, 1);
  }

  function closeDropdownOnClickOutside(event) {
    if (currentDropdown && !currentDropdown.contains(event.target) && !event.target.classList.contains('settings-btn')) {
      currentDropdown.remove();
      currentDropdown = null;
      document.removeEventListener('click', closeDropdownOnClickOutside);
    }
  }

  // Global functions for dropdown actions
  window.resetHwid = async () => {
    if (currentKey) {
      await fetch(`/api/keys/${currentKey.id}/reset`, { method: 'PATCH', headers: { 'Cookie': `connect.sid=${getCookie('connect.sid')}` } });
      loadKeys();
      closeDropdown();
    }
  };

  window.modifyLabel = async () => {
    if (currentKey) {
      const newLabel = prompt('Nuevo label:', currentKey.label);
      if (newLabel !== null) {
        await fetch(`/api/keys/${currentKey.id}`, { method: 'PATCH', headers: { 'Content-Type': 'application/json', 'Cookie': `connect.sid=${getCookie('connect.sid')}` }, body: JSON.stringify({ label: newLabel }) });
        loadKeys();
        closeDropdown();
      }
    }
  };

  window.addDays = async () => {
    if (currentKey) {
      const addDays = parseInt(prompt('Agregar días:', 0));
      if (!isNaN(addDays)) {
        await fetch(`/api/keys/${currentKey.id}`, { method: 'PATCH', headers: { 'Content-Type': 'application/json', 'Cookie': `connect.sid=${getCookie('connect.sid')}` }, body: JSON.stringify({ expiry_days: currentKey.expiry_days + addDays }) });
        loadKeys();
        closeDropdown();
      }
    }
  };

  window.deleteKey = async () => {
    if (currentKey && confirm('¿Eliminar esta licencia?')) {
      await fetch(`/api/keys/${currentKey.id}`, { method: 'DELETE', headers: { 'Cookie': `connect.sid=${getCookie('connect.sid')}` } });
      loadKeys();
      closeDropdown();
    }
  };

  window.banKey = async () => {
    if (currentKey) {
      await fetch(`/api/keys/${currentKey.id}/ban`, { method: 'PATCH', headers: { 'Content-Type': 'application/json', 'Cookie': `connect.sid=${getCookie('connect.sid')}` }, body: JSON.stringify({ banned: true }) });
      loadKeys();
      closeDropdown();
    }
  };

  window.unbanKey = async () => {
    if (currentKey) {
      await fetch(`/api/keys/${currentKey.id}/ban`, { method: 'PATCH', headers: { 'Content-Type': 'application/json', 'Cookie': `connect.sid=${getCookie('connect.sid')}` }, body: JSON.stringify({ banned: false }) });
      loadKeys();
      closeDropdown();
    }
  };

  function closeDropdown() {
    if (currentDropdown) {
      currentDropdown.remove();
      currentDropdown = null;
    }
  }

  // Make functions global
  window.showSettings = showSettings;
  window.resetHwid = resetHwid;
  window.modifyLabel = modifyLabel;
  window.addDays = addDays;
  window.deleteKey = deleteKey;
  window.banKey = banKey;
  window.unbanKey = unbanKey;

  loadKeys();
}

domReady();
