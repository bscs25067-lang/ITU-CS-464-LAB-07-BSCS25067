# ITU-CS-464-LAB-07-BSCS25067
**Student Name:** Abdullah Saeed  
**Roll Number:** BSCS25067
**Inspiration & Visual Reference:** *Resident Evil 2 Remake* (Capcom)

---

## Project Overview & Architecture
This repository contains a 10-screen mobile UI/UX menu system built in Unity, inspired by the sleek survival-horror tactical interface of *Resident Evil 2 Remake*.

## Screen-by-Screen Design Documentation

### Screen 01: Title / Splash Screen (`01_TitleScene.unity`)
* **Purpose:** Establishes the survival-horror R.P.D. terminal tone and provides a clear entry point into the main menu.
* **UI/UX Principles Applied:**
  * **Color & Contrast:** Deep slate background paired with high-contrast white text and Biohazard Crimson accents for the RE emblem.
  * **Visual Hierarchy:** Bold, center-weighted title dominates the screen while secondary system metadata is subdued in grey.
  * **Affordances & Touch Targets:** Large, bordered central prompt creates an unmistakable, thumb-friendly tap zone.
* **Tweens & Ease Curves Used:**
  * **Title Logo (`FadeInOnEnable`):** Fades in (`0 -> 1` alpha over `0.65s`) using **EaseInOutSine** for a smooth, atmospheric reveal.
  * **Start Prompt (`FadeInOutLoop`):** Fades in after a brief delay, then loops continuously between `0.25 <-> 1.0` alpha using **EaseInOutSine** to signal interactivity.
  * **Scene Transition:** Tapping triggers the additive `TransitionScene` shutter overlay using **EaseOutCubic** (close) and **EaseInCubic** (open).

<img src="Docs/Screenshots/01_Title_Annotated.png" width="600">

---

## Credits & References
* **UI Visual Direction & Color Palette Inspiration:** *Resident Evil 2 Remake* (Capcom, 2019).