# virtuelizacija-projekat

<img width="500" height="600" alt="fix4" src="https://github.com/user-attachments/assets/e0972636-084f-4d15-9923-4daa43a7f30a" /> 




Sistem je organizovan kao klijent-server aplikacija zasnovana na WCF servisu. Klijent učitava Smart Grid dataset iz CSV fajla, parsira podatke i sekvencijalno šalje uzorke servisu. Komunikacija se odvija preko tri osnovne operacije: StartSession(meta), PushSample(sample) i EndSession().

Na početku prenosa klijent poziva StartSession, nakon čega servis generiše SessionId, kreira poseban direktorijum za sesiju u okviru ServerData foldera i priprema fajlove measurements_session.csv, rejects.csv i log.txt. Ako je inicijalizacija uspešna, servis vraća ACK i status IN_PROGRESS.

Nakon toga klijent prolazi kroz CSV fajl u petlji i šalje jedan po jedan red metodom PushSample. Svaki red se predstavlja objektom SmartGridSample, koji sadrži polja Timestamp, FFT1, FFT2, FFT3, FFT4, PowerUsage i Frequency. Servis za svaki primljeni uzorak vrši validaciju. Validni uzorci se upisuju u measurements_session.csv, dok se nevalidni upisuju u rejects.csv. Nakon svake obrade servis vraća ACK ili NACK i trenutni status sesije.

Tokom prijema podataka servis vrši osnovnu analitiku nad frekvencijom i FFT karakteristikama signala. Pragovi za detekciju anomalija, odnosno F_threshold, FFT_threshold i dozvoljeno odstupanje od tekućeg proseka od ±25%, čitaju se iz konfiguracionog fajla. Ako se detektuje nagla promena frekvencije, nagla promena FFT vrednosti ili odstupanje od očekivanih vrednosti, servis podiže odgovarajući događaj koji se može koristiti za logovanje ili obaveštavanje korisnika.

Po završetku slanja svih uzoraka, klijent poziva EndSession(). Servis tada zatvara otvorene fajlove i streamove, upisuje završnu informaciju u log, postavlja status sesije na COMPLETED i vraća završni ACK.


Luka Nikolic PR131/2023 Dusan Bracika PR124/2023
