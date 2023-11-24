# Mirror
---
## Vorteile
- Interpolation funktioniert out of the box ohne harte Ruckler; Framerate runter auf ca. 30 bei 1000 GameObjects
- Steam-Kompatibler Transport mitgeliefert

## Nachteile
- Performance soll wohl eher mittelmäßig im Vergleich zu anderen Libraries sein (Netcode for GameObjects, Fishnet)
- Network-Buffers sollen in neueren Versionen zu temporären Desync führen, wenn ein Objekt vom Server-Authority zu Client-Authority oder anderherum wechselt, bis der Buffer komplett überschrieben wurde


# Netcode for Entities
---
## Vorteile
- Mehr Entities synchronisierbar; Interpolationsprobleme ab ca. 1000 Entities
- Chat- und Entity-Sync funktioniert über dieselbe Schnittstelle
- RPC-Commands ermöglichen das einfache Senden arbiträrer Daten
- Interaktion zwischen GameObjects und DOTS ohne große Probleme möglich
- Trennung von Daten und Logik kann Fehlersuche vereinfachen
- Burst-Kompatibel -> Übersetzung bestimmter Teile des Spiels in nativen Code für höhere Performance

## Nachteile
- DOTS hat noch keine built-in Animationslösung (kostenpflichtige Assets verfügbar)
- Dokumentation mangelhaft, Beispielprojekte können zum Verständnis helfen, umfassen aber nicht alle Features
- Relay auf Unity GameService ausgelegt, ob Steam-Relay verwendet werden kann unklar (und wenn ja nicht dokumentiert)
- Editor-Integration von DOTS noch nicht vollständig komfortabel nutzbar
- mehr mentaler Overhead, da man immer sicherstellen muss, dass man unmanaged Ressourcen freigibt und Änderungen an Komponenten via RefRW tätigt
- Kein Parenting von Entities möglich
- Ping scheint ungewöhnlich hoch, Grund noch unklar (evtl. Server refreshrate)
- Teile der bereits implementierten Logik müssen neu geschrieben werden
