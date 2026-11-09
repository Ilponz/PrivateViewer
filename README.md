# 🛡️ PrivateViewer (Chromium Edition)

Un'applicazione Windows moderna, autonoma e leggera progettata per proteggere finestre, note e navigazione web da qualsiasi metodo di cattura dello schermo (registrazioni video, screenshot, software di condivisione e streaming come OBS, Discord, Teams, Zoom, Meet).

---

## 🚀 Come Funziona

A differenza di strumenti che tentano l'iniezione rischiosa di DLL in processi esterni (spesso bloccati da antivirus o privi di permessi), **PrivateViewer** implementa la protezione direttamente a livello del proprio processo utilizzando l'API nativa di Windows:

```csharp
[DllImport("user32.dll", SetLastError = true)]
private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

// WDA_EXCLUDEFROMCAPTURE = 0x00000011 (Windows 10 2004+ / Windows 11)
SetWindowDisplayAffinity(this.Handle, 0x00000011);
```

### Risultato:
* **Sul tuo monitor:** La finestra è completamente visibile, reattiva e utilizzabile normalmente.
* **Nei software di cattura e registrazione:** La finestra non viene disegnata o viene completamente esclusa dal Desktop Window Manager (DWM), mostrando solo lo sfondo o le finestre sottostanti.

---

## ✨ Funzionalità

1. **Protezione Schermo Istantanea:** Attiva di default all'avvio con pulsante di stato per commutare (Attivo / Disattivo) e verificare il funzionamento.
2. **Boss Key Globale (`ESC`):** Premendo `ESC` la finestra scompare all'istante nascondendosi nell'area di notifica (System Tray). Il tasto `ESC` viene intercettato anche quando il focus è dentro la pagina web o su un campo di input. Cliccando sull'icona dello scudo nella barra di sistema la finestra viene ripristinata.
3. **Browser Chromium Moderno (Microsoft Edge WebView2):**
   * Supporto completo per standard web moderni (HTML5, JavaScript, Google, ChatGPT, YouTube, GitHub, ecc.).
   * Profilo utente e cache isolati in `%LOCALAPPDATA%\PrivateViewer\WebView2Profile`.
   * Menu contestuale nativo (copia, incolla, ispezione).
4. **Controlli di Navigazione Avanzati:**
   * Pulsanti **Indietro** (`◀`), **Avanti** (`▶`), **Ricarica / Stop** (`🔄` / `✕`), e **Home** (`🏠`).
   * **Barra URL Intelligente:** Riconosce automaticamente URL completi, domini privi di prefisso (es. `wikipedia.org`), server locali (`localhost:3000`) e ricerche testuali su Google.
   * **Sincronizzazione Titolo:** Il titolo della scheda si sincronizza in tempo reale con il titolo della pagina visitata.
   * Scorciatoie rapide: `F5` per ricaricare, `Invio` per navigare.
5. **Note Private:** Area per appunti, testi o credenziali temporanee con funzione di salvataggio su file e pulizia.
6. **Opzioni Finestra:**
   * *Sempre in primo piano:* Mantiene il visualizzatore sopra le altre finestre di lavoro.
   * *Controllo Opacità:* Slider per regolare la trasparenza dal 30% al 100%.

---

## 🛠️ Come Avviare o Ricompilare

* **Eseguibile già pronto:** Avvia direttamente `PrivateViewer.exe`.
* **Ricompilare:** Esegui `build.bat` (utilizza il compilatore nativo `csc.exe` già incluso in Windows e referenzia i moduli WebView2 inclusi).
