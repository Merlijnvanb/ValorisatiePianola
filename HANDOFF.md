# Handoff: Pianola Valorisatie

Stand van zaken op 2 november 2026. Bedoeld om een volgende werksessie (of een volgende Claude-sessie) snel op weg te helpen.

**Eerst lezen:**
- `PianolaObsidian/Midterm Voorbereiding.md`: presentatieplan, ontwerpkeuzes, criteria-check en to-do-lijst voor de midterm.
- `PianolaObsidian/Mechaniek Concreet.md`: het mechaniekplan, gebaseerd op Welte-Mignon.

## Project in het kort

- **Module:** HKU Games jaar 3, Project Valorisatie (GMS-3-PVALORI-26, 30 EC, individueel).
- **Begeleider:** Tim.
- **Stakeholder:** Pianola Museum Amsterdam. Eén contactpersoon, een oudere meneer.
- **Belangrijke data:**
  - **zo 1 nov 2026:** stakeholdergesprek, gehouden (was gepland op vr 30 okt).
  - **do 5 nov 2026:** midterm. Die is formatief en wordt gescoord met een stoplicht.
  - Uploaddeadline voor de presentatie en de urenverantwoording: deze week, exacte datum nog checken.
  - Endterm-datum en kerstvakantie: nog onbekend.
- **Ontwerpvraag v3 (5 nov):** "Hoe kan ik met een digitale pianola-installatie bezoekers een reproductiepiano authentiek laten ervaren, zowel in werking als in sfeer?"
  - Ingekort tot authenticiteit: het gaat om de werking van het hele apparaat; de pneumatiek zit in "authentiek in werking" en komt terug in de technische uitleg.
  - Definitie van wanneer de vraag behaald is: nog open.
  - Deelvragen en de verantwoording van v1 naar v2 en v3 staan in `PianolaObsidian/ProjectDoelen & Nieuwe Ontwerpvraag.md`.
  - "Zonder tekst" stond in v1 en is nu een ontwerpkeuze.
- **Doelgroep:** liefhebbers, en museumbezoekers, bijvoorbeeld op een groot scherm in de foyer.

## Concept

- **Een "open haard"-ervaring op een groot scherm.** De pianola speelt vanzelf rollen uit de kast, als ambiance. Je kunt:
  - zelf rollen kiezen;
  - in de kast kijken en het binnenwerk verkennen;
  - de tijd vertragen;
  - zelf iets inspelen met een MIDI-piano, dat als rol in de kast komt.
- **Bediening:** alles met de muis.
- **Pitchzin:** "Op normale snelheid hoor je muziek. Vertraag je de tijd, dan zie je een machine."
- **Rode draad (op advies van Tim): authenticiteit**, in twee lagen:
  - **in werking:** echte rollen, gesimuleerde pneumatiek;
  - **in sfeer:** intiem, de pianola als meubel in een huiskamer.
- **Repertoire:** Onkel Philipp's Spielzeugwerkstatt in Berlijn.

### Ontwerpkeuzes (uitgewerkt in Midterm Voorbereiding)
- **Donker en clair-obscur, niet een witte kamer in pastel.** Met veel licht lukte het intieme gevoel niet.
- **Geen speler in beeld.** De persoonlijkheid van de speler zit in de muziek en de dynamiek.
- **Het scrollwiel vertraagt de tijd**, gekoppeld aan de tempoklep.
  - Afgewezen: "klik op een noot en er speelt een animatie af".
  - Geparkeerd: de muis als lamp, en zelf ingrijpen.
- **Welte-Mignon, met focus op de pneumatiek.** Dit volgt uit de feedback van de stakeholder.
- **Zelf inspelen.** Welte was oorspronkelijk een opnamesysteem voor pianisten.
- **In de kast kijken met een obstruction shader.** Zo zie je de hele speelervaring, ook nu het binnenwerk nog niet af is.

### Mechaniek (zie Mechaniek Concreet en het implementatieplan)
- **Basis:** de rode Welte-Mignon-rol (T-100), met 100 sporen: 80 noten (C1 tot en met G7) en 10 expressiesporen aan elke kant.
- **De keten:**
  1. Een elektrische pomp met overloopklep en reservoir maakt het vacuüm.
  2. Het vacuüm gaat ongeregeld naar de luchtmotor (met governor en tempoklep), die de rol trekt.
  3. Het vacuüm gaat geregeld, via de expressie per helft, naar de stack.
  4. In de stack: pouch, dan ventiel, dan slagbalgje, dan hamer.
- **Rolformaat:** SUPRA raw MIDI. Een eigen encoder zet gewone MIDI om naar een rol.
- **Bouwvolgorde:**
  1. rollezer en stack;
  2. expressie;
  3. vacuüm;
  4. luchtmotor en governor;
  5. pedalen en terugspoelen;
  6. encoder;
  7. visualisatie.
- **Let op:** de SUPRA-rollen vallen onder CC BY-NC-SA 4.0. Dat is goed voor het prototype, maar niet voor een betaalde release. Daarvoor zijn eigen rollen via de encoder nodig, of rollen van het museum.

## Stakeholder

- **Zijn rol:** hij is **inhoudsdeskundige**, geen opdrachtgever.
- **Contact:** slecht bereikbaar per mail; bellen of langskomen werkt beter. Hij heeft weinig verstand van technologie.
- **Gespreksstijl:** hij is het snel overal mee eens. Leg daarom concrete voorstellen neer en vraag "waar zit ik ernaast?".
- **Gesprek van 1 november**, vastgelegd in `PianolaObsidian/Gesprek Pianola Museum 01-11 Feedback.pdf` en geparafeerd door beiden. Zijn feedback:
  - Focus op de pneumatiek en op wat de pianola uniek maakt tegenover een piano.
  - Laat de pomp, het reservoir, de windlade, de luchtmotor, de governor, de tempoklep en de rol zien.
  - Welte-Mignon is een goed voorbeeld.
  - Hij is enthousiast over de esthetiek en het open-haard-idee, en zoekt tekeningen uit het archief op.
- **Geparafeerde afspraken:**
  - wat Merlijn maakt blijft van Merlijn, en het museum mag het gratis gebruiken;
  - de naam van het museum mag erin;
  - school en portfolio;
  - zelf uitbrengen, **ook tegen betaling**;
  - aan het eind een beoordelingsadvies van het museum.
- **Nog vast te leggen:** een **AI-afspraak** (de module vraagt erom) en hoe de **archieftekeningen** gebruikt mogen worden. Allebei kunnen telefonisch, met datum en tijd genoteerd.
- **Aan het museum vragen:** gaat het om een rode of een groene Welte, en is het ingebouwd of een voorzetapparaat?

## Midterm

- **Demo-route:** in de kast kijken met de obstruction shader, en zelf inspelen. De opname verschijnt als rol met gaatjes en gaat de kast in. Het binnenwerk is nog niet speelbaar; dat wordt getoond via het plan.
- **Praktisch:** MIDI-keyboard meenemen, en een video als plan B.
- **De criteria-check** (alle 16 criteria) en de opbouw van de presentatie staan in `Midterm Voorbereiding.md`.
- **De grootste gaten:**
  1. de urenverantwoording loopt maar tot ongeveer 22 september;
  2. er is nog niets getest en er is nog geen testplan;
  3. de AI-afspraak en de afspraak over museummateriaal ontbreken;
  4. het doelgroeponderzoek is dun;
  5. ethiek staat nog niet expliciet uitgewerkt;
  6. de slides en de AI-bijlage (export van de Claude-gesprekken) moeten nog gemaakt worden.

## Wat er in de code staat

- **Audio:** een eigen sfizz-sampler via een native DLL (`SfizzLivePlayer`, `SfizzNative`).
- **Sequencer:** `MidiSequencer` speelt op de sample nauwkeurig en heeft `Speed`.
- **Camera en licht:** `CameraController`, `CameraPoint` met buren, en `LightManager` met lampen per plek.
- **Rollen:** `RollsManager`, `Roll` en `RollData`. De kast, een rol kiezen en afspelen werken.

## Nog te implementeren

### Voor de midterm
- De obstruction shader om in de kast te kijken.
- Zelf inspelen: MIDI-invoer opnemen, de opname als nieuwe rol in de kast, en tonen als rol met gaatjes.

### Daarna: het binnenwerk (zie de bouwvolgorde hierboven)
- **Noten doorgeven aan het beeld.** `MidiSequencer` stuurt noten nu alleen naar de audio-thread. Er moet een event komen op de main thread, of de simulatiekern uit het implementatieplan moet de visuals aansturen.
- **Scrollwiel koppelen aan de tempoklep en `Speed`.**
  - **Conflict:** `RollsManager.cs:119` gebruikt het scrollwiel al om door de kast te bladeren.
- **Plekken in het binnenwerk:**
  - pomp en reservoir;
  - luchtmotor met governor en tempoklep;
  - rol en notenbalk;
  - stack;
  - expressie;
  - hamer, kort.
  - Per plek een camerapunt en licht.

### Ambiance, art, geluid, inhoud
- Het einde van een rol opvangen, de volgende rol automatisch spelen, en een rustmodus als niemand iets doet.
- 3D-modellen van de pianola, het binnenwerk, de kast en de rollen. Eigen mist en definitieve belichting.
- Mechaniekgeluiden, eventueel opgenomen in het museum.
- Rollen: SUPRA voor het prototype; voor een release eigen of museumrollen. De testbestanden met "(c)" eruit.
- Een build voor een groot scherm, die zonder menu meteen speelt.

## Open vragen

- Heb ik een SMART-doel? Het protocol vraagt erom, en het testplan hangt eraan.
- Welke kennis uit eerdere jaren pas ik toe (playtestmethode, ontwerpmodel, onderzoeksmethode)?
- Hoe heb ik AI gebruikt, buiten de Claude-gesprekken?
- Hoe kies je in het spel een noot om te volgen?
- Wanneer zijn de endterm en de kerstvakantie, en wanneer kan een test met bezoekers in het museum?
