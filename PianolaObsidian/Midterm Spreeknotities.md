# Midterm Spreeknotities

Sprekersnotities per slide, uit het deck gehaald op 5 nov 2026. Volgorde zoals in het deck.

## 1. Cover

Het spel draaide net nog als ambiance. Aankondigen: na de presentatie mogen jullie zelf spelen. Dit is waar het vandaan komt: het Pianola Museum in Amsterdam. Gerund door vrijwilligers met passie. Die sfeer is waar mijn gevoel van authenticiteit vandaan komt. De stakeholder: de pneumatiek is het bijzondere. De mechaniek die ook in een gewone piano zit, minder. En die pneumatiek zit onzichtbaar in de kast.

## 2. Ontwerpvraag

Deze vraag is de bril voor de rest van de presentatie. Twee begrippen: authentiek in werking, en authentiek in sfeer. Werking gaat over het hele apparaat, van rol tot klank. De pneumatiek, wat het museum het belangrijkst vindt, zit daarin. Die komt terug bij de technische uitleg. De vraag is gegroeid uit de gesprekken met het museum: van muziek zichtbaar maken naar de machine authentiek laten ervaren. Kort het document tonen met de versies en waarom ze veranderden. Wanneer het voor mij gelukt is, laat ik zien bij het testplan. Wat jullie straks spelen, is mijn poging om deze vraag te beantwoorden. Eerst laat ik zien hoe die ervaring eruitziet.

## 3. De ideale ervaring (1–3)

Eerst wat de bezoeker doet. Waarom het zo is, leg ik daarna uit. De bezoeker kan altijd terug naar de hub, en de muziek stopt nooit. Bij frame 3: “Op normale snelheid hoor je muziek. Vertraag je de tijd, dan zie je een machine.” Vertragen zit in een echt bedieningsonderdeel van de pianola, de tempoklep. Een animatie na een klik voelt als een video. De obstruction shader: zo zie je nu al de hele ervaring, ook al is het binnenwerk nog niet af. Zoals in het museum de voorkant opengaat.

## 4. De ideale ervaring (4–6)

Zelf inspelen: Welte was een opnamesysteem voor pianisten. Titel, componist en uitvoerder invullen is hetzelfde als het label op een echte rol. Zo wordt de bezoeker een Welte-artiest. Een ingespeelde rol blijft in de kast. De volgende bezoeker hoort hem misschien als ambiance. Waarom de ervaring zo is: authenticiteit, in werking en in sfeer.

## 5. Authentiek in werking

Welte-Mignon bracht de opname van een echte pianist naar je huiskamer. Dat live optreden is de brug tussen werking en sfeer.
Werking: als maker weet ik dat het werkt zoals in het echt. Geen vooraf opgenomen audio, de klank wordt live gemaakt, net als bij het instrument.
Dat zagen jullie net bij frame 5 en 6: bij het inspelen merkt de bezoeker het zelf, je eigen spel komt terug als rol.

## 6. Authentiek in sfeer

Geen speler in beeld: de persoonlijkheid van de pianist zit in de muziek en de dynamiek, precies wat Welte vastlegde. Ambiance: de pianola als meubel dat vanzelf speelt, zoals in een huiskamer. Daar komt het open-haard-idee vandaan. Naar de volgende slides: hoe ik die sfeer vertaal naar beeld.

## 7. Eerst pastel, nu clair-obscur

Eerst heb ik een witte kamer in pastel geprobeerd. Met veel licht lukte het intieme gevoel niet. Clair-obscur: je bent alleen met het instrument. Het enige wat je ziet, is wat in het licht staat.

## 8. Drie woorden

Drie woorden voor de sfeer, en hoe ik ze vertaal naar beeld. Intiem is het kernwoord: je bent alleen met het instrument. Dezelfde drie woorden gebruik ik straks in het testplan om de sfeer te meten.

## 9. Moodboard

Vier bronnen voor de sfeer: clair-obscur van Pinterest, de Welte-Mignon-machines en -rollen, het museum van binnen, en Onkel Philipp’s Spielzeugwerkstatt in Berlijn. Onkel Philipp: werkende objecten, dichtbij en intiem. Kort het moodboard in PureRef tonen.

## 10. Vooronderzoek: Inhoud

Vijf typen vooronderzoek, elk op een eigen slide met de conclusie. Inhoud: SUPRA en bronnen over de Welte-Mignon, veel in het Duits en Engels. Ik heb AI gebruikt om ze samen te vatten, en daaruit mijn mechaniekplan gemaakt. Hier kort het AI-document tonen.

## 11. Vooronderzoek: Context

Context: museumbezoek en het stakeholdergesprek van 1 november. De stakeholder kijkt in het archief naar bouwtekeningen van de Welte-Mignon. Toegezegd, nog geen datum.

## 12. Vooronderzoek: Repertoire

Repertoire: Onkel Philipp in Berlijn, zo wel. Speelklok Museum Utrecht, zo niet: net te oubollig en theatraal.

## 13. Vooronderzoek: Doelgroep

Doelgroep: liefhebbers thuis, museumbezoekers, expobezoekers, mogelijk andere musea. Te checken via de stakeholder en de vrijwilligers.

## 14. Vooronderzoek: Experimenten

Experimenten: de sampler, het camerasysteem en de pastelkamer. Naar de inhoud: wat het museum het belangrijkst vindt.

## 15. Quote stakeholder

Mijn eerste beeld van het mechaniek ging vooral over wat ook in een gewone piano zit. De stakeholder stuurde bij: het bijzondere is de pneumatiek. De quote: dynamiek uit luchtdruk zichtbaar maken is nog niet eerder gedaan. Daar ligt de originaliteit van het concept, en het belang van de stakeholder. De afspraken over rechten en gebruik hebben we op 1 november samen vastgelegd en geparafeerd. Kort het formulier tonen.

## 16. Techniek

Waarom live: vanaf het begin vastgesteld dat ik goed klinkende, live gespeelde piano wilde. Vooraf opnemen heb ik overwogen, maar dan is een rol die de bezoeker zelf inspeelt niet meer echt. Waarom MIDI: een rol is al data. Gaatjes zijn noten, net als MIDI-events. Timing: de sequencer stuurt noten een stukje vooruit naar de audiothread, met de exacte sampletijd erbij. Zo klopt de timing, ongeacht de framerate. De uitdaging: de sfizz-library was verouderd. Ik heb hem zelf gecompileerd voor Windows en Linux, en de player en de MIDI-sequencer eromheen gebouwd. Hier zat veel tijd in.

## 17. De pneumatische keten

Het feit om te onthouden: hoe hard een noot klinkt, staat óók als gaatjes op de rol. Aan elke kant van de rol zitten tien expressiesporen. Bas en discant hebben elk hun eigen vacuüm, dus linker- en rechterhand kunnen verschillend hard spelen. Dat maakt het een reproductiepiano. De rol heeft 100 sporen: 80 noten en 10 expressiesporen aan elke kant. Kort het mechaniekplan tonen.

## 18. Ethiek

Drie punten die eigen zijn aan dit project. Rechten: SUPRA-rollen vallen onder CC BY-NC-SA. Voor het prototype mag dat, met naamsvermelding. Ook rollen die je online vindt hebben een licentie, dus per rol checken. Rollen uit het archief van het museum zijn een mogelijke andere bron. De testbestanden met “(c)” gaan eruit. De eigen opnames als demorollen sluiten aan op de opnamefunctie: het spel maakt zijn eigen rechtenvrije rollen. Juistheid: vereenvoudiging laat ik controleren door de stakeholder en liefhebbers, met een checklist uit het testplan. Museummateriaal: de afspraak leg ik vast bij de eerste meeting.

## 19. Testplan

Bij de midterm hoeft er nog niet getest te zijn, maar het plan moet concreet zijn.
Wat de informele tests opleverden: [conclusie game feel kast], [conclusie geluid].
De usability-test eerst, omdat navigeren de basis is voor alles wat daarna komt.

## 20. Meting ontwerpvraag

Per begrip uit de ontwerpvraag een meting, gekoppeld aan wanneer het gelukt is (vorige slide). Werking heeft twee kanten: klopt het volgens de experts, en kan de bezoeker volgen hoe het werkt. Sfeer: hoe kleiner het verschil tussen de echte ruimte en het spel, hoe authentieker. Elke test koppel ik aan een meeting in het museum, om de week.

## 21. Planning: Kanri-board

Het Kanri-board: de kolommen High, Medium en Low zijn de prioriteit, de tags het soort werk. De volgorde van het binnenwerk volgt de keten uit het mechaniekplan: eerst wat de rol leest en de noten aanstuurt, dan de lucht erachter. Scope: loopt het uit, dan vallen eerst de Low-kaarten af, daarna Medium. Ideeën als de muis als lamp heb ik bewust geparkeerd, die staan in de idee-dump.

## 22. Planning: vier fases

Vier fases in plaats van losse features: concreet genoeg om te sturen, open genoeg om binnen een fase te schuiven. De losse taken staan op het Kanri-board. Weeknummers zoals in mijn urenverantwoording: week 1 is 31 augustus, de midterm is week 10. Week 17 en 18 zijn kerstvakantie. Elke fase eindigt met een meeting in het museum, fysiek, met de test die bij die fase hoort. Bij deze stakeholder werkt een concreet voorstel met “waar zit ik ernaast?” beter dan open vragen. Bij de eerste meeting leg ik de afspraken over AI-gebruik en museummateriaal vast. Scope: loopt een fase uit, dan vallen eerst de Low-kaarten binnen die fase af. Fase 3 en 4 komen allebei uit in week 19. [Keuze: art parallel vanaf fase 1, of de encoder naar Low.] [Endterm-datum en kerstvakantie nog bevestigen.]

## 23. En nu: tijd om te spelen

Overgang naar het speelmoment. MIDI-keyboard klaar, video als plan B.
