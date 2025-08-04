/*  PickImage.jslib  –  WebGL ↔ Unity resim yükleyici + JSON indirme + URL açma desteği */

mergeInto(LibraryManager.library, {
  PickAndSend: function (goPtr, urlPtr) {
    // 0) Unity GameObject ve sunucu URL’sini UTF8 çöz
    const goName = UTF8ToString(goPtr);
    const url    = UTF8ToString(urlPtr);

    // 1) Unity instance tespit
    function getUnity() {
      if (typeof unityInstance !== "undefined") return unityInstance;
      if (typeof gameInstance  !== "undefined") return gameInstance;
      if (typeof Module !== "undefined" && typeof Module.SendMessage === "function") return Module;
      console.error("[PickAndSend] Unity instance not found");
      return null;
    }

    // 2) Gizli <input> oluştur
    const inp = document.createElement("input");
    inp.type       = "file";
    inp.accept     = "image/*";
    inp.style.display = "none";
    document.body.appendChild(inp);

    // 3) Seçim tamamlanınca sunucuya POST et
    inp.onchange = (e) => {
      const file = e.target.files[0];
      if (!file) { cleanup("No file selected"); return; }

      const fd = new FormData();
      fd.append("img", file, file.name);

      fetch(url, { method: "POST", body: fd })
        .then(r => r.json())
        .then(js => {
          console.log("[PickAndSend] lines:", js.lines ? js.lines.length : "no-key");
          const u = getUnity();
          if (u) u.SendMessage(goName, "OnJsonReceived", JSON.stringify(js));
          cleanup();
        })
        .catch(err => cleanup(err.message));
    };

    // 4) Dosya seçim diyalogunu aç
    inp.click();

    // 5) İş bittiğinde input’u kaldır
    function cleanup(err) {
      if (err) {
        const u = getUnity();
        if (u) u.SendMessage(goName, "OnJsonReceived", `{"error":"${err}"}`);
      }
      if (inp.parentNode) document.body.removeChild(inp);
    }
  },

  // ──────────── Buradan itibaren ayrı bir üye────────────
  DownloadFile: function (fileNamePtr, contentPtr) {
    const fileName = UTF8ToString(fileNamePtr);
    const content  = UTF8ToString(contentPtr);

    const blob     = new Blob([content], { type: 'application/json' });
    const a        = document.createElement('a');
    a.href         = URL.createObjectURL(blob);
    a.download     = fileName;
    a.click();
    URL.revokeObjectURL(a.href);
  },

  /* ─── Yeni Eklenen Fonksiyon ───
     OpenTab: WebGL'de yeni sekmede URL açmak için.
     Unity C# tarafında:
          [DllImport("__Internal")]
          private static extern void OpenTab(string url);
   */
  OpenTab: function (urlPtr) {
    const url = UTF8ToString(urlPtr);
    window.open(url, '_blank');
  }

  // Eğer başka fonksiyonlar varsa, buraya ,
  // UploadFile: function(...) { … }
});
