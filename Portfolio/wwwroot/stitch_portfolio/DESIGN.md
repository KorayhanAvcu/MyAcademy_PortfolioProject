---
name: Lumina Portfolio
colors:
  surface: '#101415'
  surface-dim: '#101415'
  surface-bright: '#363a3b'
  surface-container-lowest: '#0b0f10'
  surface-container-low: '#191c1e'
  surface-container: '#1d2022'
  surface-container-high: '#272a2c'
  surface-container-highest: '#323537'
  on-surface: '#e0e3e5'
  on-surface-variant: '#c7c4d7'
  inverse-surface: '#e0e3e5'
  inverse-on-surface: '#2d3133'
  outline: '#908fa0'
  outline-variant: '#464554'
  surface-tint: '#c0c1ff'
  primary: '#c0c1ff'
  on-primary: '#1000a9'
  primary-container: '#8083ff'
  on-primary-container: '#0d0096'
  inverse-primary: '#494bd6'
  secondary: '#c8c6c5'
  on-secondary: '#313030'
  secondary-container: '#4a4949'
  on-secondary-container: '#bab8b7'
  tertiary: '#ffb783'
  on-tertiary: '#4f2500'
  tertiary-container: '#d97721'
  on-tertiary-container: '#452000'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#e1e0ff'
  primary-fixed-dim: '#c0c1ff'
  on-primary-fixed: '#07006c'
  on-primary-fixed-variant: '#2f2ebe'
  secondary-fixed: '#e5e2e1'
  secondary-fixed-dim: '#c8c6c5'
  on-secondary-fixed: '#1c1b1b'
  on-secondary-fixed-variant: '#474646'
  tertiary-fixed: '#ffdcc5'
  tertiary-fixed-dim: '#ffb783'
  on-tertiary-fixed: '#301400'
  on-tertiary-fixed-variant: '#703700'
  background: '#101415'
  on-background: '#e0e3e5'
  surface-variant: '#323537'
typography:
  display-lg:
    fontFamily: Inter
    fontSize: 72px
    fontWeight: '800'
    lineHeight: 80px
    letterSpacing: -0.04em
  display-lg-mobile:
    fontFamily: Inter
    fontSize: 40px
    fontWeight: '800'
    lineHeight: 48px
    letterSpacing: -0.02em
  headline-md:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '600'
    lineHeight: 40px
    letterSpacing: -0.02em
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  label-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.05em
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  container-max: 1200px
  gutter: 24px
  section-padding-desktop: 120px
  section-padding-mobile: 64px
  stack-sm: 8px
  stack-md: 16px
  stack-lg: 32px
---

## Brand & Style

The design system is centered on a **Modern Minimalist** aesthetic tailored for high-end professional portfolios. It prioritizes clarity, authority, and technical sophistication. By leveraging a dark-mode-first approach, the system directs intense focus toward content and imagery, evoking a sense of premium craftsmanship and digital precision.

The target audience includes hiring managers, creative directors, and potential clients who value attention to detail and a "less is more" philosophy. The emotional response should be one of calm confidence and streamlined efficiency. The visual language utilizes heavy whitespace (or "dark space") to create breathing room between dense professional records and high-impact project visuals.

## Colors

This design system utilizes a high-contrast dark palette to establish a cinematic professional tone.

*   **Primary (Electric Indigo):** Used sparingly for calls-to-action, active states, and critical highlights to draw the eye without overwhelming the content.
*   **Background (Deep Charcoal):** A rich, non-pure black that provides depth and reduces eye strain while maintaining high contrast for white text.
*   **Surface:** A slightly lighter charcoal used for cards and section layering to create a sense of physical hierarchy.
*   **Typography:** Crisp white (#FFFFFF) for primary headings and a muted slate-gray (#94A3B8) for secondary body text to maintain a clear visual hierarchy.

## Typography

The typography relies entirely on **Inter** to achieve a systematic, "neo-grotesque" look that feels both neutral and highly legible. 

*   **Headlines:** Utilize tight letter-spacing and heavy weights (Bold/ExtraBold) to create a strong "editorial" impact.
*   **Body Text:** Set with generous line-height to ensure readability against the dark background.
*   **Labels:** Use all-caps with increased letter-spacing for small metadata elements like dates in the Experience section or categories in the Portfolio.
*   **Scalability:** Display sizes scale down aggressively for mobile to ensure hero statements remain impactful without causing horizontal overflow.

## Layout & Spacing

The system follows a **Fixed Grid** philosophy for desktop to maintain a premium, composed feel, transitioning to a fluid model for mobile devices.

*   **Grid:** A 12-column grid with a 24px gutter. Content is typically centered within a 1200px max-width container.
*   **Section Vertical Spacing:** Large 120px gaps between major sections (About, Services, Portfolio) to enforce the minimalist aesthetic and give each content block total focus.
*   **Mobile Adaptation:** Margins shrink to 20px on mobile, and the 12-column desktop grid collapses to a 1-column stack. Section padding is halved to maintain momentum while scrolling.

## Elevation & Depth

Depth is achieved through **Glassmorphism** and tonal layering rather than traditional heavy shadows.

*   **Tonal Layers:** The base background is #121212. Elements "closer" to the user use #1E1E1E.
*   **Glassmorphism:** Cards (Portfolio items, Testimonials) use a semi-transparent background (`rgba(30, 30, 30, 0.6)`) with a `backdrop-filter: blur(12px)`. 
*   **Borders:** Each elevated element features a subtle 1px solid border (`rgba(255, 255, 255, 0.1)`) to define edges against the dark background without adding visual weight.
*   **Accents:** A soft, low-opacity glow of Electric Indigo may be used behind primary project images to create an "ambient light" effect.

## Shapes

The shape language is **Soft** and disciplined. 

*   **Components:** Buttons and input fields use a 0.25rem (4px) radius to maintain a professional, sharp-edged look while avoiding the harshness of 90-degree corners.
*   **Containers:** Portfolio cards and larger surface areas use `rounded-lg` (8px) to subtly distinguish them from the base layout.
*   **Media:** Professional imagery and project thumbnails should follow the `rounded-lg` constraint to maintain a cohesive "frame" throughout the portfolio.

## Components

*   **Buttons:** Primary buttons are solid Electric Indigo with white text. Secondary buttons are "Ghost" style—1px white borders with a subtle hover state that increases background opacity.
*   **Portfolio Cards:** Use the glassmorphic style described in Elevation. Images should occupy the top half of the card, with a "Label" component for the category and "Headline-MD" for the title below.
*   **Services List:** A clean vertical stack with `border-bottom` separators. Each item features an icon in Electric Indigo and a brief body description.
*   **Experience Timeline:** A minimal vertical line (1px width, rgba(255,255,255,0.2)) with small indigo dots for milestones. 
*   **Input Fields:** Darker than the surface (`#0A0A0A`), with a 1px border. On focus, the border transitions to Electric Indigo with a 2px outer glow.
*   **Testimonials:** Centered typography within a glassmorphic card, utilizing "Body-LG" for the quote and "Label-SM" for the attribution.