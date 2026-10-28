# Handoff: Pianola Valorisatie

Stand van zaken op 28 oktober 2026. Bedoeld om een volgende werksessie (of een volgende Claude-sessie) snel op weg te helpen.

## Project in het kort

- **Module:** HKU Games jaar 3, Project Valorisatie (GMS-3-PVALORI-26, 30 EC, individueel).
- **Stakeholder:** Pianola Museum Amsterdam. Eén contactpersoon, een oudere meneer.
- **Belangrijke data:**
  - vr 30 okt 2026: stakeholdergesprek;
  - do 5 nov 2026: midterm. Die is formatief en wordt gescoord met een stoplicht.
- **Ontwerpvraag:** "Hoe kan ik een digitale pianola-installatie maken die de gebruiker inzicht geeft in haar interne werking, zonder tekstblokken te gebruiken?"
  - Herformuleren wordt overwogen, zodat het sfeeraspect erin zit.

## Concept

- **Een "open haard"-ervaring op een groot scherm.** De pianola speelt vanzelf rollen uit de kast, als ambiance. Word je nieuwsgierig, dan kun je:
  - zelf rollen kiezen;
  - het binnenwerk verkennen;
  - eventueel zelf iets opnemen met een MIDI-piano.
- **Bediening:** alles met de muis. Het toetsenbord is alleen nodig voor de naam van een opname.
- **Pitchzin:** "Op normale snelheid hoor je muziek. Vertraag je de tijd, dan zie je een machine."

### Gekozen interactie
- **Het scrollwiel vertraagt de tijd.** Dat gaat via `MidiSequencer.Speed` en dezelfde tijdschaal voor de animaties.
- **Je navigeert tussen plekken in het binnenwerk** en kunt zo één toon volgen, van het gaatje tot de hamer. Elke plek heeft een eigen camerapunt en spotlight (clair-obscur).
- **Het binnenwerk wordt live aangestuurd door de sequencer**, niet met vooraf gemaakte animaties.
- **Afgewezen:** "klik op een noot en er speelt een animatie af". Dat voelt als een video.
- **Geparkeerd:** de muis als lamp of röntgenbril, en zelf ingrijpen (bijvoorbeeld de pomp stilzetten).

### Het binnenwerk, zoals voorgesteld aan de stakeholder
Het instrument is waarschijnlijk een pianola met een motortje, niet met trappers.

1. **Pomp:** het motortje laat de pomp lopen en er ontstaat onderdruk.
2. **Rol:** de rol schuift over de notenbalk.
3. **Ventiel:** er stroomt lucht door het gaatje en het ventiel gaat open.
4. **Balgje:** het balgje klapt dicht.
5. **Hamer:** de hamer raakt de snaar.
6. **Terug in rust.**

Hard en zacht staat vermoedelijk op de rol (reproductiepiano). Dat moet vrijdag bevestigd worden.

## Stakeholder: aanpak

- **Zijn rol:** hij is **inhoudsdeskundige**, geen opdrachtgever. Hij levert kennis en controleert of de inhoud klopt. Merlijn is de ontwerper.
- **Contact:** slecht bereikbaar per mail; bellen of langskomen werkt beter. Hij heeft weinig verstand van technologie, dus geen jargon gebruiken.
- **Gespreksstijl:** hij heeft geen eigen eisen en is het snel overal mee eens. Daarom stuurt Merlijn het gesprek, en werkt dit goed:
  - concrete voorstellen neerleggen;
  - vragen "waar zit ik ernaast?";
  - laten zien in plaats van vertellen;
  - zelf samenvatten.
- **Vastleggen:** op papier, met per onderdeel een feedbackregel, geparafeerd, plus een foto als bewijs. In het verslag eerlijk opschrijven: "door mij voorgesteld, door de stakeholder bevestigd".
- **Gespreksblad voor vrijdag:** `Documenten/Gesprek Pianola Museum 30-10 v2.odt`.
  - Pagina 1: het idee, plus het voorstel voor het binnenwerk met feedback per onderdeel.
  - Pagina 2: "Mijn plan", met feedbackregels.
  - Pagina 3: afspraken en paraaf. De afspraken gaan over:
    - eigendom, en gratis gebruik door het museum;
    - de naam van het museum;
    - school en portfolio;
    - zelf uitbrengen (gratis of ook betaald);
    - een beoordelingsadvies aan het eind.
  - `Documenten/Gesprek Pianola Museum 30-10.odt` is de eigen versie van vóór de aanpassingen. De `.docx`-bestanden in `Other/` zijn verouderde versies.

## Midterm

- **Bronnen:** `ModuleInfo/Protocol Student Valorisatie 26-27.pdf` (criteria en verloop) en `ModuleInfo/Project Valorisatie Osiris module-omschrijving.docx` (leerdoelen en AI-regels).
- **Verloop:** 15 min presentatie, 5 min spelen, 5 min vragen. Vooraf uploaden: de presentatie en de urenverantwoording.
- **AI-regel:** wordt AI als sparringpartner gebruikt, dan moet het **hele gesprek** in een AI-bijlage. Dat geldt ook voor de Claude-sessies.

### Wat er staat
- **Technologie:**
  - eigen sfizz-sampler via een native DLL;
  - een sequencer die op de sample nauwkeurig speelt, met `Speed`;
  - camerasysteem en lampen per plek;
  - rollenkast, plus rol kiezen en afspelen.
- **Concept:** concept, pitchzin en gekozen interactie, met een afgewezen alternatief als onderbouwing.

### Wat vrijdag oplevert
- Feedback op het voorstel voor het binnenwerk en op het plan.
- Geparafeerde afspraken over rechten, uitbrengen en naamgebruik.
- Datums voor volgende afspraken. Probeer meteen een testdatum in het museum vast te leggen.

### Gaten om vóór 5 november te dichten
1. **Urenverantwoording bijwerken.** Die loopt maar tot ongeveer 22 september (`PianolaObsidian/Urenverantwoording.md`).
2. **Greybox van het binnenwerk:** 3 à 4 plekken, live bewegend, met het scrollwiel dat de tijd vertraagt. Dit is ook het speelmoment.
3. **Kleine test** met 2 à 3 mensen. Vraag daarna: "Hoe ontstaat een toon?" Lukt dat niet, dan een concreet testplan.
4. **Vooronderzoek aanvullen:**
   - repertoire: 3 à 4 voorbeelden van "zo werkt het"-visualisaties en sfeerervaringen;
   - doelgroep.
5. **Ethiek expliciet maken:**
   - de MIDI-bestanden van derden (sommige met "(c)") zijn alleen testmateriaal;
   - nauwkeurigheid tegenover erfgoed.
6. **AI-bijlage:** de Claude-gesprekken exporteren.
7. **Presentatieplanning** in feature-voor-featurevorm, met controlemomenten. Nog uit te werken; de endterm-datum en de vakantie zijn nog onbekend.

## Nog te implementeren

### 1. Binnenwerk (de kern, nodig voor de ontwerpvraag)
- **Noten doorgeven aan het beeld.** `MidiSequencer` stuurt noten nu alleen naar de audio-thread. Er moet een event komen op de main thread (noot aan, noot uit) waar de visuals op reageren.
- **Scrollwiel koppelen aan `MidiSequencer.Speed`**, met een minimum en maximum en een vloeiende overgang. Animaties volgen dezelfde tijdschaal.
  - **Conflict:** `RollsManager.cs:119` gebruikt het scrollwiel al om door de kast te bladeren. Bepaal per context wat het wiel doet.
- **Plekken als greybox:**
  - de rol en de notenbalk, met gaatjes die meeschuiven;
  - het ventiel;
  - het balgje;
  - de hamer en de snaar;
  - de pomp en het motortje (achtergrond).
- **Live aansturing per onderdeel:** bij noot aan klapt het balgje dicht en slaat de hamer; bij noot uit gaat alles terug in rust.
- **Camerapunten en licht per plek**, via `CameraPoint` en `LightManager`.
- **Eén toon volgen:** het licht volgt een noot van plek naar plek.
- **Hard en zacht laten zien.** Wacht op de uitleg van de stakeholder.

### 2. Ambiance-modus
- Het einde van een rol opvangen en automatisch de volgende rol spelen.
- Een overgang bij het wisselen van rol.
- Een rustmodus als niemand iets doet, waarin de camera langzaam rondgaat.

### 3. Sfeer en art
- Een 3D-concept en -model van de pianola en van de onderdelen van het binnenwerk.
- Een 3D-model van de kast, plus de rol met variatie en polish.
- Eigen mist.
- Definitieve belichting in clair-obscur.

### 4. Geluid
- Mechaniekgeluiden die hoorbaar worden als je de tijd vertraagt.
- Opnames in het museum, als dat mag.

### 5. Inhoud
- Rollen van het museum, of muziek die vrij te gebruiken is, in plaats van de testbestanden.
- `RollData` invullen: titel, componist, uitvoerder.

### 6. Afronding
- Een build voor een groot scherm, volledig met de muis te bedienen.
- Opstarten zonder menu: het spel speelt meteen.

### 7. Alleen als er tijd over is
- Opnemen met een MIDI-piano, een naam invoeren, en de opname als nieuwe rol in de kast.

## Open vragen
- Hoe kies je een noot om te volgen? Klik je zelf op een gaatje, of volgt het licht vanzelf de volgende noot?
- Hoe lang duurt het in het echt van gaatje tot hamer, in milliseconden? Vragen aan de stakeholder.
- Hoe zit het precies met hard en zacht op deze pianola?
- Wanneer is de endterm, wanneer is de kerstvakantie, en wanneer kan een test met bezoekers in het museum?
