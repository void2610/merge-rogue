mergeInto(LibraryManager.library, {
  CopyWebGL: function(str) {
    if(navigator.clipboard){
      navigator.clipboard.writeText(str) //httpでは使えない(httpsのみ)
      .then(function(text){
    });
    }else{
      var str = Pointer_stringify(str);
      var listener = function(e){
        e.clipboardData.setData("text/plain" , str);
        e.preventDefault();
        document.removeEventListener("copy", listener);
      }
      document.addEventListener("copy" , listener);
      document.execCommand("copy");
    }
  },

  PasteWeb: function() {
    if(navigator.clipboard){
    navigator.clipboard.readText()
    .then(function(text){
        SendMessage('CopyPaste', 'paste', text);
        //クリップボードから取得したテキストを渡す（ゲームオブジェクト名，メソッド名，クリップボードの値）
    });
    }
  }
});
