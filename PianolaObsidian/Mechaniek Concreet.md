Gebaseerd op Welte-Mignon
Rode rol (T-100): 100 sporen, middelste 80 voor noten, 10 expressiesporen aan elke kant
De piano zelf heeft gewoon 85/88 toetsen; de rol speelt alleen C1–G7 (MIDI 24–103). Op 88 toetsen blijven de onderste 3 (A0–B0) en bovenste 5 (G#7–C8) stil: die hebben geen balgje in de stack. Groene Welte ging later naar 88 noten
Encoder: MIDI-noten buiten dit bereik een octaaf verschuiven (zoals arrangeurs voor de rol deden), weglaten, of naar 88 gaan
Vragen aan museum: rode of groene Welte? Ingebouwd of voorzetapparaat?

Midi encoder:
Baseer op SUPRA raw formaat (aparte sporen voor bastonen, discanttonen, basexpressie en discantexpressie)
Zet gewone MIDI om naar een rol: noten worden gaatjes (iets vóór de onset, vanwege de aanslagvertraging), velocity wordt expressiegaatjes per helft, CC64/CC67 worden pedaalgaatjes. Bouw ik als laatste; eerst testen met gescande SUPRA-rollen

Elektrische pomp:
Vast tempo per rol, ongeregeld richting luchtmotor en geregeld richting windlade voor de notenstack
Overloopklep begrenst het maximale vacuüm (start: 35 inch waterkolom), reservoir vangt pieken op: een groot akkoord geeft een korte dip die herstelt

Notenstack (windlade):
Gesplitst in bas en discant, elk met een eigen vacuümniveau
Per noot: gaatje open → pouch → ventiel → slagbalgje klapt dicht → hamer. Aanslagkracht volgt uit het vacuüm van die helft
Gaatje dicht → lucht via de bleed terug → balgje loopt vol → noot los

Expressie:
Per helft één crescendobalg (stand 0–1) die het vacuüm en dus de velocity bepaalt
Gestuurd door expressiegaatjes: langzame crescendo aan/uit, MF-haak, forzando forte/piano
Regels overnemen uit PlaySK (MIT, met naamsvermelding)

Luchtmotor + governor:
Trekt de rol. Draait op ongeregeld vacuüm en vertraagt dus bij luide passages; de governor knijpt de luchtstroom af zodat het tempo gelijk blijft

Tempoklep:
Eén vaste stand per rol ("Normal")
Idee: scrollwiel als tempoklep om de tijd te vertragen

Pedalen en terugspoelen:
Zelfde keten als een noot (gaatje, pouch, ventiel, balg). Sustain en soft via losse aan/uit-gaatjes, terugspoelen aan het eind van de rol; de stack wordt dan afgesloten

Volgorde:
1. Rollezer + stack: SUPRA-rol speelt op vaste sterkte
2. Expressie
3. Pomp, overloopklep en reservoir
4. Luchtmotor + governor
5. Pedalen + terugspoelen
6. Encoder
7. Visualisatie koppelen

Let op:
SUPRA-rollen zijn CC BY-NC-SA 4.0: goed voor studie en prototype, niet voor een betaalde release. Daarvoor eigen rollen via de encoder (rechtenvrije stukken) of rollen van het museum
