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