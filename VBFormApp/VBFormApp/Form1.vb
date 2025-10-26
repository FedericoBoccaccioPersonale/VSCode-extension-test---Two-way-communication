Imports System.IO
Imports System.IO.Pipes
Imports System.Threading

Public Class Form1
	Private contatore As Integer = 0
	Private pipeWriter As StreamWriter = Nothing
	Private ReadOnly pipeLock As New Object() ' Oggetto per la sincronizzazione dei thread

	Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles Me.Shown
		LblStato.Text = "Avvio del server pipe..."
		Log("Form avviato. Avvio del server pipe...")
		Dim pipeThread As New Thread(AddressOf AvviaPipeServer)
		pipeThread.IsBackground = True
		pipeThread.Start()
	End Sub

	Private Sub AvviaPipeServer()
		' Questo While True dovrebbe servire per gestire la riconnessione in caso di caduta
		While True
			Dim pipeServer As NamedPipeServerStream = Nothing
			Dim sr As StreamReader = Nothing
			Dim sw As StreamWriter = Nothing

			Try
				pipeServer = New NamedPipeServerStream("vbformpipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous)
				Log("Pipe creata. In attesa di connessione...")
				AggiornaStato("In attesa di connessione...")

				pipeServer.WaitForConnection()
				AggiornaStato("Client connesso")
				Log("Connessione ricevuta.")

				' Gli strumenti di lettura e scrittura
				sr = New StreamReader(pipeServer)
				sw = New StreamWriter(pipeServer)
				sw.AutoFlush = True
				Me.pipeWriter = sw

				While pipeServer.IsConnected
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

			Catch ex As Exception
				Log("Errore pipe: " & ex.Message)
				AggiornaStato("Errore nella pipe")

			Finally
				If sw IsNot Nothing Then sw.Close()
				If sr IsNot Nothing Then sr.Close()
				If pipeServer IsNot Nothing Then pipeServer.Close()
				Me.pipeWriter = Nothing ' Pulisce lo stream quando il client si disconnette o in caso di errore
				Log("Client disconnesso.")
				AggiornaStato("Client disconnesso")
			End Try
		End While
	End Sub


	Private Sub Log(msg As String)
		If txtLog IsNot Nothing Then
			If txtLog.InvokeRequired Then
				txtLog.Invoke(Sub() txtLog.AppendText(msg & Environment.NewLine))
			Else
				txtLog.AppendText(msg & Environment.NewLine)
			End If
		End If
	End Sub

	Private Sub btnInvia_Click(sender As Object, e As EventArgs) Handles btnInvia.Click
		Dim writer As StreamWriter = Me.pipeWriter ' Copia il riferimento per un uso sicuro nel thread
		Log("Pulsante premuto. Invio messaggio...")
		If writer IsNot Nothing Then
			Log("Pulsante premuto. Invio messaggio...")
			Task.Run(Sub()
						 Log("Pulsante premuto. Invio messaggio...")
						 Try
							 Dim messaggio As String = "Messaggio dal Form! " & DateTime.Now.ToLongTimeString()
							 ' Scrittura Thread-Safe
							 Log("Prima di SyncLock")
							 SyncLock pipeLock
								 Log("Dentro SyncLock, prima di WriteLine")
								 writer.WriteLine(messaggio)
								 Log("Dopo WriteLine")
								 writer.Flush()
								 Log("Dopo Flush")
							 End SyncLock
							 Log("Fine invio")

							 Log("Messaggio inviato con successo dal background thread.")
						 Catch ex As IOException
							 Log("Errore di I/O durante l'invio (client probabilmente disconnesso): " & ex.Message)
						 Catch ex As Exception
							 Log("Errore imprevisto nel task di invio: " & ex.Message)
						 End Try
					 End Sub)
		Else
			Log("Nessun client connesso. Impossibile inviare.")
		End If
	End Sub

	Private Sub AggiornaStato(stato As String)
		If LblStato.InvokeRequired Then
			LblStato.Invoke(Sub() ImpostaTestoStato(stato))
		Else
			ImpostaTestoStato(stato)
		End If
	End Sub

	''' <summary>
	''' Questo è l' ennesimo GROSSOLANO ERRORE DI PROGETTAZIONE di .NET!
	''' Non permette di espandere il testo allineandolo da destra!
	''' </summary>
	''' <param name="testo"></param>
	Private Sub ImpostaTestoStato(testo As String)
		LblStato.Text = testo

		' Calcola la larghezza necessaria per il testo
		Dim larghezzaTesto As Integer = TextRenderer.MeasureText(testo, LblStato.Font).Width

		' Imposta la larghezza della label in base al testo
		LblStato.Width = larghezzaTesto

		' Mantiene la label ancorata al bordo destro del form
		LblStato.Left = Me.ClientSize.Width - LblStato.Width - 10 ' margine destro

		' Cambia colore in base allo stato - una enum sarebbe stata meglio ma questo è un esperimento
		Select Case testo.ToLowerInvariant()
			Case "client connesso"
				LblStato.ForeColor = Color.Green
			Case "client disconnesso", "errore nella pipe"
				LblStato.ForeColor = Color.Red
			Case Else
				LblStato.ForeColor = Color.Black
		End Select
	End Sub


End Class