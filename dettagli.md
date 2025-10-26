Una delle cose che ODIO di markdown è che converte automaticamente in link qualunque cosa gli somigli, come ad esempio "VB.NET".
Inoltre non permette di aprire i link in una nuova scheda e non va a capo.

# Illogico funzionamento

## Using
```
Private Sub AvviaPipeServer()
	' Il While True dentro a un While True serve per gestire la disconnessione
	While True
		Try
			Using pipeServer As New NamedPipeServerStream("vbformpipe", PipeDirection.InOut, 1)
				Log("Pipe creata. In attesa di connessione...")
				AggiornaStato("In attesa di connessione...")
				pipeServer.WaitForConnection()
				AggiornaStato("Client connesso")
				Log("Connessione ricevuta.")

				' Gli strumenti di lettura e scrittura
				Using sr As New StreamReader(pipeServer), sw As New StreamWriter(pipeServer)
					sw.AutoFlush = True
					Me.pipeWriter = sw

					While True
						Dim msg As String = sr.ReadLine()
						If msg Is Nothing Then Exit While
						Log("Messaggio ricevuto: " & msg)

						contatore += 1
						Dim risposta As String = If(contatore Mod 2 = 0, "pari", "dispari")
						' SyncLock è l' istruzione di sincronizzazione, ottiene l' accesso esclusivo alla risorsa
						SyncLock pipeLock
							sw.WriteLine(risposta)
						End SyncLock
						Log("Risposta inviata: " & risposta)
					End While
				End Using

			End Using
			Log("Client disconnesso.")
			AggiornaStato("Client disconnesso")
		Catch ex As Exception
			Log("Errore pipe: " & ex.Message)
			AggiornaStato("Errore nella pipe")
		Finally
			Me.pipeWriter = Nothing ' Pulisce lo stream quando il client si disconnette o in caso di errore
		End Try
	End While
End Sub
```
Per ragioni che non riesco a comprendere non funziona, anche se mi è più volte stato consigliato dall' IA. Il pulsante di invio messaggio si bloccava continuamente dopo il terzo `Log("Pulsante premuto. Invio messaggio...")` senza indicare alcun errore. Quindi ritengo che questo succedesse perchè questa forma di codice blocca la pipe e l' invio di messaggi.

Non sono affatto sicuro che il problema sia using, ritengo invece il problema sia il loop infinito interno.

Sebbene anche adesso c' è `sr.ReadLine()` ma non è più bloccante.

## Efficienza
Per quanto assurdo, è meglio usare un while true per la gestione dei messaggi perchè dovrebbe autosospendersi fino all' arrivo di un nuovo messaggio. Un Timer invece controllerebbe di continuo, anche quando non serve.

## Altri metodi che avrei potuto provare
* **Creare un nuovo evento personalizzato da intercettare con `client.on`**. Ammesso che sia possibile, inoltre questo avrebbe generato due modi distinti di comunicare dal form: col pipe in risposta, e tramite evento alla pressione del pulsante. *Tuttavia* sarebbe comunque stata una gestione ad eventi che non occupa troppo il processore.
* **Creare un timer che controlla periodicamente se ci sono messaggi**. Questo non sono sicuro che funzionerebbe davvero, oltre ad essere inefficientissimo. Bisognerebbe evitare che il controllo della presenza di messaggi sia bloccante. In questo caso il pulsante avrebbe potuto inviare.
* **Mettere un timer in js**: implementare una complessa architettura simile a master slave, dove il js a intervalli regolari invia un messaggio a vb col solo scopo di attivarlo, oltre ovviamente ad attivarsi quando viene davvero premuto il pulsante. Poi vb controlla se ha messaggi in memoria da inviare e nel caso li invia in risposta al controllo. In questo modo il pulsante non tenterebbe più direttamente l' invio ma scriverebbe il messaggio in una coda che verrebbe letta dalla coppia di while-true prima di rispondere.

# Considerazioni
Adesso **pretendo** che venga istituito il Nobel per l' Informatica col solo scopo di potermelo conferire!
Perchè non è possibile perdere settimane intere per una cosa così semplice e che sia più facile comunicare con le Voyager piuttosto che far comunicare due processi sulla stessa macchina!

Ho provato a far realizzare questo microprogetto da copilot, per scoprire come si fa a far comunicare i processi, solo che come al solito:
* L' interfaccia nel browser è pesantissima e continua a non renderizzare correttamente la pagina.
* Addirittura delle volte il mouse spariva quando passava sulla finestra.
* Blocca il browser.
* Poi scriveva in ritardo nel campo del prompt e nemmeno sempre accettava l' input.
* Non sempre permetteva di copiare il codice generato
* Ha generato innumerevoli codici che neanche compilano
* Ha scritto funzioni che poi non ha usato o ha cercato di accedere a funzioni inesistenti
* E poi mi dà fastidio che il pulsante "Copia" sia fisso in alto nel codice.
* Scrive le risposte una riga alla volta, anche se ha già tutto pronto subito. E poi si interrompe mentre scrive, lasciando sotto dello spazio vuoto.
* È schizofrenico: ha più volte cambiato idea sul fatto che fosse meglio usare using piuttosto che istruzioni sequenziali.
* È stato schizofrenico anche per quanto riguarda il OutputType tra WinExe e Exe.

È tanto intelligente che manca solo che risponda "roger-roger".

Ci ho provato anche con phind.com ma continuava a dare errore.

Jules ha fallito.

## Open source
Copilot è profondamente **ipocrita**!
Tanto per cominciare, piuttosto che andare a cercare una soluzione già esistente su internet ha inventato abomini non funzionanti, poi, quando gli ho chiesto "Possibile che nessun programma open source faccia già questo? Così da potergli copiare il sorgente?" ha ipocritamente detto "Quando cerco progetti open source o articoli online, non posso copiare direttamente il codice sorgente completo da quei siti per motivi di copyright e rispetto delle licenze.".
Inoltre ormai avevo terminato il programma.

Ha elencato:

🔧 1. [Named Pipe Wrapper for .NET 4.0 (GitHub)](https://github.com/acdvorak/named-pipe-wrapper)

- **Libreria open source** che semplifica l’uso delle Named Pipes in C#/.NET
- Include esempi con GUI (`ExampleGUI`) e CLI (`ExampleCLI`)
- Supporta comunicazione bidirezionale e serializzazione di oggetti
- Può essere facilmente adattata a VB.NET

È open source, quindi copiabile.
Ha licenza MIT, quindi copiabile.
Non lo testo perchè non si adatta precisamente alla comunicazione con un estensione, ma copilot avrebbe dovuto copiarne il funzionamento.

📘 2. [Microsoft Learn – Named Pipes in .NET](https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-use-named-pipes-for-network-interprocess-communication)

- Documentazione ufficiale con **esempi completi** di `NamedPipeServerStream` e `NamedPipeClientStream`
- Mostra come creare server e client con comunicazione full-duplex
- Ottimo punto di partenza per costruire una soluzione personalizzata

Anche questo non è un esempio non è completo, ma avrebbe potuto copiarlo

🧪 3. [CodeProject – Asynchronous Named Pipes in VB.NET](https://www.codeproject.com/articles/Asynchronous-Named-Pipes-Overlapped-IO-with-VB-NET)

- Articolo dettagliato con codice sorgente in VB.NET
- Spiega come gestire più client e usare I/O asincrono
- Include gestione avanzata degli stream e threading

Sempre e solo parziale

🧩 4. [StackOverflow – NamedPipeServerStream in VB.NET](https://stackoverflow.com/questions/50239378/vb-net-named-pipes-namedpipeserverstream-stackoverflowexception)

- Discussione su problemi reali con Named Pipes in VB.NET
- Contiene frammenti di codice e soluzioni a errori comuni
- Utile per evitare blocchi e eccezioni silenziose

Mi astengo dal commentare

🔍 5. [StreamJsonRpc con Named Pipes (ASP.NET Core)](https://anthonysimmon.com/local-ipc-over-named-pipes-aspnet-core-streamjsonrpc-dotnet/)

- Approccio moderno con **RPC su Named Pipes**
- Utile se vuoi strutturare la comunicazione con contratti e messaggi
- Più avanzato, ma adattabile anche a desktop app

Questo non ha senso, forse ha copiato questo ed essendo ASP non è compatibile con applicazioni form.

**In definitiva**, non esistono, o almeno non ha trovato, estensioni VSC che comunicano con un form VB.NET, quindi questo progetto ha comunque valore.

## Git
Continua a darmi una lunga serie di file non sincronizzati.