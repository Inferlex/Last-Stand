# Last Stand – Počítačová hra žánru RTS

Real-time strategie vytvořená v rámci bakalářské a diplomové práce na
Katedře informatiky PřF UP Olomouc. Hráč staví budovy, těží suroviny, a za pomoci nich vytváři a ovládá jednotky.
Diplomová práce hru rozšířila o multiplayer přes internet a nový pohyb
jednotek ve skupinách.

## Klíčové vlastnosti

- **Multiplayer klient–server**: Unity Netcode for GameObjects, Lobby a Relay.
  Vytváření, zranění a odstranění jednotek a budov běží na serveru.
- **Vlastní lockstep synchronizace**: místo pozic se posílají jen příkazy
  hráčů ve vlastním textovém protokolu, s dávkováním, potvrzováním a
  opětovným odesláním při nesouladu. Pravidelná kontrolní synchronizace stavu.
- **Paralelní fronty zpráv**: tři „roury“ řízené stavovým automatem
  (ready, ongoing, confirm, clear, proceed) se zachovaným pořadím zpracování.
- **Hledání cest**: FlowField s hierarchií tilů a portálů (Dijkstra, BFS) a vyhýbání
  jednotek pomocí boidů.

## Ukázka hry

<img width="1624" height="920" alt="Snímek obrazovky 2026-10-14 132333" src="https://github.com/user-attachments/assets/6027a78e-5620-4e9c-a3b3-8172cce61ac5" />
<img width="1628" height="913" alt="Snímek obrazovky 2026-10-14 132409" src="https://github.com/user-attachments/assets/b0632650-fba6-471a-8aa2-778ce2894694" />
<img width="1626" height="921" alt="Snímek obrazovky 2026-10-14 132447" src="https://github.com/user-attachments/assets/79269b22-91bc-4271-a9a9-9d8d4b9fcb0c" />

  
## Odkazy

- Diplomová práce: [Pokročilé prvky RTS hry a jejich implementace](https://theses.cz/id/17gijd/) (2026)
- Bakalářská práce: [Počítačová hra žánru RTS](https://theses.cz/id/7zn2cy/) (2023)

Autor: Ondřej Fremuth
