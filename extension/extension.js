const path = require('path');
const vscode = require('vscode');
const { spawn } = require('child_process');
const net = require('net');

let formProcess = null;
let formRunning = false;
let pipeClient = null; // Client persistente

// La chiamata viene definita in fondo, per qualche illogica ragione
function activate(context)
{
	// Creo l' icona nella barra di stato
	const statusBarItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left);
	statusBarItem.text = '$(rocket) VB.NET Form';
	statusBarItem.command = 'vbnet.sendMessage';
	statusBarItem.show();

	const command = vscode.commands.registerCommand('vbnet.sendMessage', () =>
	{
		// Se il form non è in esecuzione, avvialo e connettiti
		if (!formRunning)
		{
			const exePath = path.join(
				__dirname,
				'..',
				'VBFormApp',
				'VBFormApp',
				'bin',
				'Debug',
				'net8.0-windows',
				'VBFormApp.exe'
			);

			vscode.window.showInformationMessage(`Avvio form: ${exePath}`); // Messaggio popup in basso a destra in VSC
			formProcess = spawn(exePath, [], { shell: true });
			/*
				exePath: è il percorso dell'eseguibile che vuoi avviare.
				[]: è un array di argomenti da passare all'eseguibile (vuoto in questo caso).
				{ shell: true }: indica che il comando deve essere eseguito in una shell (utile per comandi come dir, ls, ecc.).
			 */

			// spawn viene eseguito all' avvio dell' applicazione figlia
			// In alcuni casi dovrebbe anche essere possibile usare nomi di evento personalizzati
			// process.stdout.on('data', (data) => e process.stderr.on('data', (data) => dovrebbero leggere lo standard input e standard output
			formProcess.on('spawn', () =>
			{
				vscode.window.showInformationMessage('Form VB.NET avviato');
				formRunning = true;
				// Avvia la connessione persistente
				connectAndListen();
			});

			formProcess.on('error', (err) =>
			{
				vscode.window.showErrorMessage(`Errore nell'avvio del form: ${err.message}`);
				formRunning = false;
			});

			formProcess.on('exit', () =>
			{
				vscode.window.showInformationMessage('Form VB.NET chiuso');
				formRunning = false;
				if (pipeClient)
				{
					pipeClient.destroy();
					pipeClient = null;
				}
			});
		}
		else
		{
			// Se il form è in esecuzione, invia semplicemente un messaggio
			if (pipeClient && pipeClient.writable)
			{
				// È necessario \n finale
				// In questa implementazione funziona con qualunque messaggio
				pipeClient.write('incrementa\n');
				vscode.window.showInformationMessage('Messaggio inviato tramite connessione esistente.');
			}
			else
			{
				vscode.window.showWarningMessage('Nessuna connessione attiva. Tento di riconnettermi...');
				connectAndListen();
			}
		}
	});

	context.subscriptions.push(command);
	context.subscriptions.push(statusBarItem);
}

function connectAndListen()
{
	let retries = 0;
	const maxRetries = 10;
	const retryDelay = 500;
	const pipePath = '\\\\.\\pipe\\vbformpipe'; // Le pipe non dovrebbero creare veri file, limitando l' uso del disco

	/**
	 * Per quanto sia del tutto privo di logica definire una funzione incapsulata dentro un' altra, in questo modo
	 * * Non è chiamabile esternamente
	 * * Accede automaticamente alle variabili
	 * Dovrebbe essere possibile dichiarare la funzione anche dopo la chiamata.
	 * @returns
	 */
	function tryConnect()
	{
		if (pipeClient && pipeClient.writable)return; // Se siamo già connessi, non fare nulla
		
		const client = net.connect({ path: pipePath });

		client.on('connect', () =>
		{
			vscode.window.showInformationMessage('Connesso alla pipe. Pronto per la comunicazione bidirezionale.');
			pipeClient = client; // Salva il client persistente

			// La connessione è ora pronta per ricevere ed inviare messaggi.
			// L'invio avverrà solo con i click successivi dell'utente.

			// Listener per i dati in arrivo dal form
			pipeClient.on('data', (data) =>
			{
				vscode.window.showInformationMessage(`Dati dal form: ${data.toString()}`);
			});

			// Listener per la chiusura della connessione
			pipeClient.on('end', () =>
			{
				vscode.window.showInformationMessage('Connessione con il form chiusa.');
				pipeClient = null;
			});
		});

		client.on('error', (err) =>
		{
			client.destroy();
			if (retries < maxRetries)
			{
				retries++;
				setTimeout(tryConnect, retryDelay);
			}
			else
			{
				vscode.window.showErrorMessage(`Impossibile connettersi al form dopo ${maxRetries} tentativi.`);
				pipeClient = null;
			}
		});
	}

	tryConnect();
}


function deactivate()
{
	if (pipeClient)
	{
		pipeClient.end();
		pipeClient = null;
	}
	if (formProcess)formProcess.kill();
}

module.exports = { activate, deactivate };