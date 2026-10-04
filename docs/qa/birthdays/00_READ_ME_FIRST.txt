QA: birthday scenes (T-107). One scene per villager, on their birthday, once a year, with a choice, a gift back and a thank-you letter next morning.
Use a DEVELOPMENT build or the Editor with a throwaway game. F1 opens the console. Paste ONE LINE at a time. Wait for a map to finish loading before "scene".

forced_<npc>_birthday.txt    Plays the scene now. Take each of the three choices on separate runs; the closing line and the reply change.
natural_<npc>_birthday.txt   Jumps to the birthday (the date is forward only: use a fresh game, or add a year with "day" when the date has passed) and walks into the map.
                             The scene starts on its own when the villager is on the map and the shop or window is open (see QA_HEART_EVENTS for each villager window).
missed_<npc>_birthday_letter.txt  Sets the birthday, then sleeps one day without going to the villager; the next morning a "missed birthday" letter is in the mailbox.
                             (Without having celebrated: the letter only comes when the scene was not seen this year.)

CHECK: the closing line follows the choice (flags choice.bday.<npc>.*); the gift appears in the pack; a thank-you letter arrives next day; the scene appears in Memories as "<Name>'s Birthday";
the same scene does not repeat the same day, and does come back next year; the calendar shows the birthday; the reminder letter arrives one or two days before (hearts 3 or more).
