QA: courtship without marriage (T-109). Optional, all-ages, never required. Marriage is out of scope for 1.0.
Eight villagers can be courted: Wren, Hazel, Juno, Piper, Felix, Dorian, Elara, Ione. Tilda, Bram, Marcus and Dr. Penn are not (their heart-10 scenes stay).
Use a DEVELOPMENT build or the Editor with a throwaway game. F1 opens the console. Paste ONE LINE at a time.

<npc>_ask.txt       Plays the ask. Take each of the three answers on separate runs: yes, stay friends, not yet.
<npc>_partners.txt  Plays the partners scene (needs a yes). Take each of the three evenings.

CHECK: yes sets flag choice.courtship.<npc>.yes; friends closes the offer for good; not yet brings one more ask at 10 hearts (<npc>_courtship2);
only one courtship at a time (a yes with someone else blocks the others); the partners scene sets partners.<npc>, the Neighbours page then shows the stage "Partners",
and three partner-only talk lines can be heard. Nothing is shown or required with HorrorLevel or any setting: the game is complete without it.
