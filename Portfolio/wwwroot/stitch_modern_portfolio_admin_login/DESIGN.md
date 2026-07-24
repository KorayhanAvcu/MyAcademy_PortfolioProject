---
name: Executive Interface
colors:
  surface: '#f7f9fb'
  surface-dim: '#d8dadc'
  surface-bright: '#f7f9fb'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f2f4f6'
  surface-container: '#eceef0'
  surface-container-high: '#e6e8ea'
  surface-container-highest: '#e0e3e5'
  on-surface: '#191c1e'
  on-surface-variant: '#434655'
  inverse-surface: '#2d3133'
  inverse-on-surface: '#eff1f3'
  outline: '#737686'
  outline-variant: '#c3c6d7'
  surface-tint: '#0053db'
  primary: '#004ac6'
  on-primary: '#ffffff'
  primary-container: '#2563eb'
  on-primary-container: '#eeefff'
  inverse-primary: '#b4c5ff'
  secondary: '#505f76'
  on-secondary: '#ffffff'
  secondary-container: '#d0e1fb'
  on-secondary-container: '#54647a'
  tertiary: '#005a82'
  on-tertiary: '#ffffff'
  tertiary-container: '#0074a6'
  on-tertiary-container: '#e4f2ff'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#dbe1ff'
  primary-fixed-dim: '#b4c5ff'
  on-primary-fixed: '#00174b'
  on-primary-fixed-variant: '#003ea8'
  secondary-fixed: '#d3e4fe'
  secondary-fixed-dim: '#b7c8e1'
  on-secondary-fixed: '#0b1c30'
  on-secondary-fixed-variant: '#38485d'
  tertiary-fixed: '#c9e6ff'
  tertiary-fixed-dim: '#89ceff'
  on-tertiary-fixed: '#001e2f'
  on-tertiary-fixed-variant: '#004c6e'
  background: '#f7f9fb'
  on-background: '#191c1e'
  surface-variant: '#e0e3e5'
typography:
  headline-xl:
    fontFamily: Geist
    fontSize: 36px
    fontWeight: '600'
    lineHeight: 44px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Geist
    fontSize: 28px
    fontWeight: '600'
    lineHeight: 36px
    letterSpacing: -0.01em
  headline-lg-mobile:
    fontFamily: Geist
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  headline-md:
    fontFamily: Geist
    fontSize: 20px
    fontWeight: '500'
    lineHeight: 28px
  body-lg:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  label-md:
    fontFamily: Geist
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
    letterSpacing: 0.05em
  code:
    fontFamily: Geist
    fontSize: 13px
    fontWeight: '400'
    lineHeight: 18px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  base: 4px
  xs: 8px
  sm: 12px
  md: 16px
  lg: 24px
  xl: 32px
  2xl: 48px
  3xl: 64px
  gutter: 24px
  container-max: 1440px
---

## Brand & Style

This design system targets high-end portfolio management and administrative oversight. The brand personality is **authoritative, precise, and transparent**. It leverages a **Corporate Modern** aesthetic influenced by the structured elegance of high-growth fintech platforms (like Stripe) and the functional rigor of Material Design 3.

The visual narrative is built on the following pillars:
- **Clarity of Data:** Utilizing expansive whitespace to allow complex metrics to breathe.
- **Linear Precision:** Sharp alignment and subtle hair-line borders that suggest technical accuracy.
- **Premium Tactility:** Soft transitions and micro-interactions that make the administrative experience feel responsive and high-value.
- **Modern Professionalism:** A balance of utility and sophistication, moving away from "legacy corporate" toward a clean, developer-adjacent "SaaS-premium" look.

## Colors

The palette is anchored by **#2563eb (Blue 600)** as the primary brand driver, used for calls to action, active states, and focus indicators. The background is strictly **#f8fafc (Slate 50)** to provide a soft, low-strain canvas that differentiates itself from the pure white **(#ffffff)** surfaces of cards and modal containers.

- **Primary:** High-intent actions and brand identity.
- **Surface:** Used for content containers to create a layered "stacking" effect against the background.
- **Neutral/Secondary:** Used for secondary text and decorative icons to maintain a calm visual hierarchy.
- **Borders:** Subtle **#e2e8f0** lines define the structure without creating visual noise, emulating the "Linear" style of design.

## Typography

This design system uses a dual-font strategy. **Geist** is utilized for headlines, labels, and UI numbers due to its technical precision and monospaced-influenced kerning, which suits tabular data and administrative titles. **Inter** is used for body copy to ensure maximum readability and a softer, more approachable feel for long-form information.

Hierarchy is maintained through strict weight differentiation. Headlines should remain SemiBold (600) to stand out against the light UI, while labels use Medium (500) with slight letter spacing to denote secondary navigation or metadata categories.

## Layout & Spacing

The design follows a **12-column fluid grid** for desktop, transitioning to a single-column stack for mobile devices. 

- **Desktop (1280px+):** 24px margins, 24px gutters.
- **Tablet (768px - 1279px):** 16px margins, 16px gutters.
- **Mobile (Up to 767px):** 16px margins, 12px gutters.

The vertical rhythm is governed by a **4px baseline grid**. Components should prioritize large internal padding (minimum 24px for cards) to convey a premium, spacious feel. Section spacing should be generous, typically utilizing **32px or 48px** to separate distinct administrative modules.

## Elevation & Depth

The design system employs **Tonal Layering** and **Ambient Shadows** to create a sense of organized depth. Surfaces do not "float" aggressively; instead, they sit naturally within the environment.

- **Level 0 (Background):** Slate 50. Flat.
- **Level 1 (Cards/Tables):** White surface with a 1px border (#e2e8f0) and a subtle "Stripe-style" shadow: `0px 1px 3px rgba(0,0,0,0.02), 0px 4px 6px rgba(0,0,0,0.03)`.
- **Level 2 (Modals/Dropdowns):** White surface with a more pronounced elevation: `0px 10px 15px -3px rgba(0,0,0,0.05), 0px 4px 6px -2px rgba(0,0,0,0.02)`.
- **Interactions:** On hover, cards may lift slightly by increasing shadow spread and darkening the border tint marginally.

## Shapes

The design system uses a **Rounded** shape language to soften the corporate aesthetic and provide a modern, friendly interface. 

- **Standard Components:** Buttons and inputs use a **0.5rem (8px)** radius.
- **Large Containers:** Dashboard cards and main content areas use a **1rem (16px)** radius to emphasize the "premium" feel.
- **Full Rounding:** Progress bars, tags, and specific status badges use pill-shapes (9999px) to contrast against the structured grid.

## Components

### Buttons
Primary buttons feature a subtle vertical gradient (from Primary to a slightly darker shade) and a faint inner highlight on the top edge to create a tactile, pressed-glass effect. Text is Geist Medium. Secondary buttons are ghost-styled with a 1px border.

### Input Fields
Influenced by Material Design 3, inputs use a solid #f8fafc fill with a bottom-accent border that transforms into a full 2px primary border on focus. Icons are placed leading (left) for context and should be 20px in size with a secondary color tint.

### Cards
The cornerstone of the admin panel. Cards must have a 16px corner radius, a white background, and a 1px slate border. Headers within cards should be separated by a subtle horizontal rule or a tonal background change.

### Chips & Badges
Used for status indicators (e.g., "Active", "Pending"). These use low-saturation background colors with high-saturation text of the same hue to ensure legibility without being distracting.

### Navigation
The sidebar uses a "translucent" approach with a blurred background or a clean white surface with active states indicated by a thick 4px vertical bar on the left edge and a subtle primary-tinted background highlight.