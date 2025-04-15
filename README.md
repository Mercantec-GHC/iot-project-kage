# H3 IoT Project

## Pitch perfect:
Vi vil lave en **IoT platform**, som gør brug af disse hardwarekomponenter: **Arduino MKR IoT carrier med en MKR WiFi 1010, og bruge nogle af sensorerne på carrieren**. Vores mål er at udvikle en løsning, der kan **lave målinger med sensoren, som så skal kunne vises skærmen af arduino-enheden, der har lavet målingen. Så skal man kunne se målingerne, og redigere i enhederne fra et dashboard/hjemmeside/mobilapp**, ved at integrere et interaktivt system, der kan indsamle og reagere på data. For at opnå dette, tænker vi at anvende **Blazor til dashboard/hjemmeside, ASP.NET Core som API med Websockets til kommunikation fra arduinoen til databasen, Docker server til at hoste web elementer og holde en database,**  til at opbygge systemet, som vil tillade os at **gemme historik af data fra arduino-enheder på en database. For at give mulighed for grafer på vores dashboard.** 

Vores system vil kunne interagere med brugerne gennem et dashboard, der viser **informationer og lokationer af enheder**, hvilket giver brugerne mulighed for at **interagere med enhederne og dens komponenter.** Dette projekt vil ikke kun give os praktisk erfaring med **realtids kommunikation mellem arduino-enheder og database via API med Websockets,** men også muligheden for at udforske, hvordan teknologi kan anvendes til at løse reelle problemer eller forbedre dagligdagen.

## Case Beskrivelse:
En virksomhed, der leverer smart devices til private og erhvervskunder, ønsker at skabe en mere sammenhængende og brugervenlig oplevelse for sine kunder. Udover at sælge selve enhederne – såsom temperatur-, fugtigheds- og bevægelsessensorer samt aktuatorer til lys og motorer – har virksomheden behov for en platform, hvor kunderne kan konfigurere og styre deres enheder centralt.

**Virksomhedens kunder mangler et samlet system til at:**
* Få overblik over deres smart devices og sensorer
* Tilgå historiske data som temperatur- og bevægelses målinger
* Konfigurere placering af enhederne i specifikke rum eller zoner
* Opsætte regler og automatiseringer på tværs af enheder
* Få beskeder og sende kommandoer til deres enheder
* Tilgå det hele både fra en computer og mobiltelefon 

*Der skal bruges login for at kunne tilgå hjemmesiden/appen, samt at kunne konfigurere nye enheder.*

## Kravspecifikation:
### 1. Funktionelle krav (hvad systemet skal kunne)
#### 1.1 Enheds overblik
* Systemet skal vise en oversigt over alle brugerens tilsluttede smart devices (f.eks. sensorer og aktuatorer).
* Brugeren skal kunne se detaljer for hver enhed: type, navn, status, placering og eventuel sidste aktivitet.
#### 1.2 Historiske data
* Brugeren skal kunne tilgå historiske målinger pr. enhed, fx temperatur, luftfugtighed og bevægelse.
* Data skal kunne vises grafisk over tid (kurver/diagrammer).
* Det skal være muligt at vælge periode (dage eller uger).
* Gemmes i databasen i maks 14 dage
#### 1.3 Konfiguration og placering
* Brugeren skal kunne navngive og placere enheder i specifikke rum eller zoner (fx “Køkken”, “Stue”).
* Brugeren skal kunne flytte enheder fra ét rum/zone til et andet.
#### 1.4 Automatisering og regler
* Brugeren skal kunne oprette automatiseringsregler, f.eks. "Hvis bevægelse registreres i stuen, tænd lys".
* Brugeren skal kunne definere betingelser (if), handlinger (then), og evt. tidsrum eller gentagelse.
* Systemet skal kunne håndtere regler på tværs af forskellige enheder.
#### 1.5 Notifikationer og beskeder 
* Brugeren skal kunne få notifikationer (fx push eller email) ved bestemte hændelser.
#### 1.6 Platform og adgang
* Systemet skal være tilgængeligt både som hjemmeside og mobilapp (iOS/Android).
* Brugeren skal kunne logge ind med brugernavn og adgangskode (evt. 2-faktor-login).
* Brugeren skal kunne registrere og konfigurere nye enheder ved hjælp af Bluetooth (eller hoste hjemmeside på selve enheden).
* Brugeren skal kunne sende kommandoer til enheder (fx. Sluk lys via knap)

### 2. Ikke-funktionelle krav (hvordan systemet skal være)
#### 2.1 Brugervenlighed
* Systemet skal være intuitivt og nemt at bruge for både private og erhvervsbrugere.
* UI/UX skal tilpasses til både mobil og desktop.
#### 2.2 Sikkerhed
* Al kommunikation mellem klient og server skal ske via krypterede forbindelser (HTTPS, kommunikation mellem enheder og API under udviklingsfasen vil ikke være krypteret).
* Data (herunder historiske målinger) skal lagres sikkert og være beskyttet mod uautoriseret adgang.

### 3. Systemkrav og integrationer
#### 3.1 Backend og datalagring
* Skal understøtte realtidsdata og historisk datalagring.
* Skal have API til kommunikation med enhederne (IoT integration).
#### 3.2 Enhedsregistrering og kommunikation
* Systemet skal kunne opdage og registrere nye enheder 
* Platformen skal kunne sende og modtage kommandoer fra/til enhederne.
