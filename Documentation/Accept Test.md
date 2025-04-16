# Accept Test
## 1.1 Enheds overblik
### Test 1.1.1 – Visning af tilsluttede enheder
* **Forudsætningen:** Brugeren er logget ind og har mindst én tilsluttet enhed.
* **Handling:** Naviger til “Enheds overblik” siden på dashboardet.
* **Forventet resultat:** Alle tilsluttede enheder vises i en liste eller oversigt.
* **Fejl:** Har brugeren ingen enheder, vil de se en tom liste.

### Test 1.1.2 – Visning af enheds detaljer
* **Forudsætningen:** Brugeren er logget ind og har mindst én tilsluttet enhed.
* **Handling:** Klik på en enhed i oversigten for at se detaljer for enheden.
* **Forventet resultat:** Detaljer, viser: type, navn, status, placering og sidste aktivitet.
* **Fejl:** Brugeren får en besked om at de ikke har rettighed til denne enhed.

## 1.2 Historiske data
### Test 1.2.1 – Tilgå historiske målinger
* **Forudsætning:** Enheder har genereret data mindst en gang.
* **Handling:** Tryk på “Historik” ud for den enhed, du ønsker at se historikken på.
* **Forventet resultat:** Graf/diagram vises med målinger over tid (fx temperatur/luftfugtighed).
* **Fejl:** Enheden har ikke genereret noget data endnu.

### Test 1.2.2 – Vælg periode
* **Forudsætning:** Enheden har genereret data mindst en gang.
* **Handling:** Vælg visning af data for “1 dag” og “1 uge”.
* **Forventet resultat:** Graf opdateres korrekt ift. valgt periode.
* **Fejl:** Enheden har ikke genereret noget data endnu.

### Test 1.2.3 – Data slettes efter 14 dage
* **Forudsætningen:** Data ældre end 14 dage findes i databasen.
* **Handling:** Automatisk oprydning af data ældre end 14 dage sker søndag midnat.
* **Forventet resultat:** Ingen data ældre end 14 dage vises.
* **Fejl:** Manuel maintenance skal udføres.

## 1.3 Konfiguration og placering
### Test 1.3.1 – Oprettelse af rum
* **Forudsætningen:** Brugeren vil opdele sit dashboard i rum.
* **Handling:** Tryk på knappen “Opret nyt rum”.
* **Forventet resultat:** Naviger til “opret rum” siden.
* **Fejl:** Rummet allerede eksisterer. 

### Test 1.3.2 – Navngivning og placering
* **Forudsætningen:** Brugeren skal have en ny enhed og rummet “Køkken” skal være oprettet.
* **Handling:** Giv en enhed navnet “Sensor køkken” og placer i rum “Køkken”.
* **Forventet resultat:** Ændringer gemmes og vises i oversigten.
* **Fejl:** Rummet “Køkken” er ikke oprettet.

### Test 1.3.3 – Flytning af enhed
* **Forudsætning:** Rummet “Stue” skal være oprettet.
* **Handling:** Flyt enheden fra “Køkken” til “Stue”.
* **Forventet resultat:** Enheden vises under “Stue” i oversigten.
* **Fejl:** Rummet “Stue” er ikke oprettet.

## 1.4 Automatisering og regler
### Test 1.4.1 – Opret automatiseringsregel
* **Forudsætningen:** Brugeren vil lave en regel.
* **Handling:** Opret regel: “Hvis bevægelse registreres i stuen, så tænd lys”.
* **Forventet resultat:** Regel oprettes og vises i regeloversigten.
* **Fejl:** Enheden har ikke den rigtige sensor til implementering af den nye regel.

### Test 1.4.2 – Regel med tidsbetingelse
* **Forudsætningen:** Brugerne vil lave en regel som kun skal køre mellem “kl. 18 og 22”.
* **Handling:** Tilføj tidsrum “mellem kl. 18 og 22”.
* **Forventet resultat:** Regel fungerer kun i angivet tidsrum.
* **Fejl:** Ukendt fejl.

## 1.5 Notifikationer og beskeder
### Test 1.5.1 – Notifikation ved hændelse
* **Forudsætning:** Regel oprettet med notifikation.
* **Handling:** Udløser hændelse (fx bevægelse).
* **Forventet resultat:** Push eller e-mail notifikation modtages.
* **Fejl:** Mail eller push er ikke konfigureret.

## 1.6 Platform og adgang
### Test 1.6.1 – Tilgængelighed på platforme
* Forudsætning: Brugeren skal altid kunne tilgå sit dashboard og sine enheder.
* **Handling:** Åbn systemet på både mobil (iOS/Android) og browser.
* **Forventet resultat:** Systemet fungerer på begge platforme.
* **Fejl:** Brugerens udstyr er forældet og kan ikke tilgå sit dashboard og sine enheder.

### Test 1.6.2 – Login med 2-faktor
* **Forudsætningen:** Brugerne vil have yderligere sikkerhed over deres enheder.
* **Handling:** Log ind med brugernavn, kode og 2-faktor.
* **Forventet resultat:** Bruger bliver korrekt logget ind.
* **Fejl:** Brugeren kan ikke få adgang til sin authenticator app.

### Test 1.6.3 – Tilføj enhed via Bluetooth
* *Forudsætning:* Brugeren har købt en ny enhed, som de vil have sat op.
* **Handling:** Start parring med ny enhed via Bluetooth.
* **Forventet resultat:** Enheden bliver fundet og tilføjet systemet.
* **Fejl:** Kan ikke forbinde via Bluetooth.

### Test 1.6.4 – Send kommando til enhed
* **Forudsætningen:** Brugeren vil f.eks. Kunne tænde sit lys ved hjælp af sit dashboard.
* **Handling:** Tryk på “On/Off lys”-knap.
* *Forventet resultat:* Kommando sendes, og enheden reagerer.
* **Fejl:** Ukendt fejl *“Enheden kan have mistet forbindelsen til internettet”*.

## 2. Systemkrav og integration
### Test 2.1.1 – Realtidsdata og historik
* **Forudsætningen:** Enheden har internetforbindelse.
* *Handling:* Observer live data og derefter historik.
* **Forventet resultat:** Realtidsopdateringer vises, og historik kan tilgås.
* **Fejl:** Enheden har ikke internetforbindelse.

### Test 2.1.2 – API kommunikation
* **Forudsætning:** Enheden har internetforbindelse.
* **Handling:** Enheden sender data til systemet via API.
* **Forventet resultat:** Data modtages korrekt og vises i systemet.
* **Fejl:** Enheden har ikke internetforbindelse.

### Test 2.2.1 – Opdag og registrer ny enhed
* **Forudsætningen:** Enheden er blevet konfigureret rigtigt via bluetooth.
* **Handling:** Tænd en ny enhed.
* **Forventet resultat:** System opdager og tilbyder registrering.
* **Fejl:** Enheden er ikke blevet konfigureret.

### Test 2.2.2 – Send/modtag kommandoer
* **Forudsætning:** Brugeren vil sende kommandoer.
* **Handling:** Send “tænd/sluk”-kommando til enheden.
* **Forventet resultat:** Enheden reagerer straks.
* **Fejl:** Enheden kan have mistet internetforbindelsen. 
