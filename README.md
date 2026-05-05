# Virtuelizacija-projekat

Projekat implementira nadzor Smart Grid sistema kroz klijent-server arhitekturu. 
Klijent učitava CSV dataset i sekvencijalno šalje merenja WCF servisu. 
Servis validira podatke, snima validna merenja u measurements_session.csv, nevalidna u rejects.csv, prati stanje sesije i računa anomalije nad frekvencijom i FFT karakteristikama. 
Kada se detektuje važna promena, servis podiže događaje koji se koriste za logovanje i obaveštavanje.

Luka Nikolic PR131/2023 Dusan Bracika PR124/2023
