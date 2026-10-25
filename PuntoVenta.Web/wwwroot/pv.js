// Imprime un documento HTML completo usando un iframe oculto (no cambia ni tapa la pagina actual)
window.pvImprimir = function (html) {
    var f = document.createElement('iframe');
    f.style.position = 'fixed';
    f.style.right = '0';
    f.style.bottom = '0';
    f.style.width = '0';
    f.style.height = '0';
    f.style.border = '0';
    document.body.appendChild(f);
    var d = f.contentWindow.document;
    d.open();
    d.write(html);
    d.close();
    setTimeout(function () {
        try { f.contentWindow.focus(); f.contentWindow.print(); } catch (e) { }
        setTimeout(function () { if (f.parentNode) f.parentNode.removeChild(f); }, 3000);
    }, 300);
};

// ---- Sesion sincronizada entre pestanas ----
// La sesion (JWT) se guarda en localStorage con la clave "sesion_pv".
// Si en otra pestana se cierra sesion, esta pestana pasa al login.
(function () {
    var CLAVE = 'sesion_pv';

    function leer() {
        try { return window.localStorage.getItem(CLAVE); } catch (e) { return null; }
    }

    function irAlLogin() {
        if (window.location.pathname.toLowerCase() !== '/login') {
            window.location.replace('/login');
        }
    }

    var ultima = leer();

    // 1) Aviso inmediato del navegador cuando otra pestana cambia la sesion
    window.addEventListener('storage', function (e) {
        if (e.key !== CLAVE) return;
        if (e.newValue === null) {
            irAlLogin();                                   // se cerro sesion en otra pestana
        } else if (e.oldValue !== null && e.newValue !== e.oldValue) {
            window.location.reload();                      // entro otro usuario: tomar la sesion nueva
        }
        ultima = e.newValue;
    });

    // 2) Respaldo: revisa cada 2 segundos y al volver a la pestana
    function revisar() {
        var actual = leer();
        if (ultima !== null && actual === null) {
            ultima = actual;
            irAlLogin();
            return;
        }
        ultima = actual;
    }
    setInterval(revisar, 2000);
    document.addEventListener('visibilitychange', function () { if (!document.hidden) revisar(); });
    window.addEventListener('focus', revisar);
})();