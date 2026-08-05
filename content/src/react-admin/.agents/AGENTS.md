# AI Agent Workspace Rules

## UI Design Specifications Guide

When working on UI components, layouts, or styling tasks in this repository, **all AI Agents must read and adhere to the project UI specifications**:

- Primary UI Guide: [prompts/ui.md](prompts/ui.md)

### UI Rule Summary
- Colors: Primary `#4361ee`, Secondary `#4895ef`, Title `#111827`, Body `#374151`, Bg `#f7f9fc`, Card `#ffffff`, Border `#e5e7eb`. Brand gradients are prohibited.
- Color source: literal hex/RGB/HSL colors may only be declared as `:root` tokens in `src/index.css`. CSS Modules, JSX/TSX, and the Ant Design theme must consume global CSS variables instead of duplicating color literals.
- Spacing: 8px grid (8px, 16px, 24px, 32px, 48px, 64px).
- Border Radii: Card 12px, Button/Input 8px.
- Boundary: Only modify layout, styling, and interactive states. Do not change business logic or API structures.
