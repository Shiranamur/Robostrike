window.localStore = {
  set: function(key, value) {
    localStorage.setItem(key, value);
  },
  get: function(key) {
    return localStorage.getItem(key);
  },
  remove: function(key) {
    localStorage.removeItem(key);
  },
  setJson: function (key, obj) {
    const json = JSON.stringify(obj);
    localStorage.setItem(key, json);
  },
  getJson: function (key) {
    const json = localStorage.getItem(key);
    return json ? JSON.parse(json) : null;
  }
};