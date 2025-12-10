# Converting This Presentation to PowerPoint

## Method 1: Using Marp (Recommended)

### Install Marp CLI
```bash
npm install -g @marp-team/marp-cli
```

### Convert to PowerPoint
```bash
marp PRESENTATION.md --pptx -o EFCore10-Presentation.pptx
```

### Convert to PDF
```bash
marp PRESENTATION.md --pdf -o EFCore10-Presentation.pdf
```

### Marp Directives (Add to top of PRESENTATION.md)
```markdown
---
marp: true
theme: default
paginate: true
backgroundColor: #fff
backgroundImage: url('https://marp.app/assets/hero-background.svg')
---
```

---

## Method 2: Using Slidev (Interactive Web Slides)

### Install Slidev
```bash
npm init slidev
```

### Copy PRESENTATION.md content
1. Create new Slidev project
2. Replace `slides.md` with PRESENTATION.md content
3. Run: `npm run dev`

### Features
- Live code editing
- Presenter notes
- Recording support
- Export to PDF/PPTX

---

## Method 3: Manual PowerPoint Creation

### Slide Structure

#### Slide 1: Title
- Title: "EF Core 10 Deep Dive"
- Subtitle: "What's NEW in Entity Framework Core 10"
- Your name, date
- GitHub link

#### Slide 2: Agenda
- Bullet list of 7 topics
- Use numbers
- Bold key features

#### Slide 3: Why EF Core 10
- 5 key themes (icons if available)
- Technology stack badges
- .NET 10 logo

#### Slides 4-7: Complex Types (4 slides)
- Slide 4: The Problem (code)
- Slide 5: The Solution (code)
- Slide 6: Configuration (code)
- Slide 7: When to Use (table)

#### Slides 8-10: ExecuteUpdate (3 slides)
- Slide 8: The Problem (code)
- Slide 9: The Solution (code)
- Slide 10: Performance Table

#### Slides 11-16: Vector Search (6 slides)
- Slide 11: What is Vector Search
- Slide 12: Product Model (code)
- Slide 13: How It Works
- Slide 14: Distance Metrics
- Slide 15: Demo Results
- Slide 16: Use Cases

#### Slides 17-20: Named Query Filters (4 slides)
- Slide 17: The Problem (code)
- Slide 18: The Solution (code)
- Slide 19: Configuration (code)
- Slide 20: Demo Results

#### Slides 21-23: LeftJoin (3 slides)
- Slide 21: The Problem (code)
- Slide 22: The Solution (code)
- Slide 23: Before/After Comparison

#### Slides 24-25: Parameterized Collections (2 slides)
- Slide 24: The Problem & Solution
- Slide 25: Demo Results

#### Slide 26: Performance Summary
- Big numbers table
- Use colors (green for improvements)

#### Slides 27-29: Architecture (3 slides)
- Slide 27: Strategy Pattern
- Slide 28: Clean Architecture
- Slide 29: Tech Stack

#### Slides 30-32: Takeaways (3 slides)
- Slide 30: Key Takeaways
- Slide 31: Real-World Use Cases (table)
- Slide 32: Choose the Right Tool

#### Slide 33: Resources
- Links with QR codes
- Documentation
- GitHub repo

#### Slide 34: Live Demo
- Terminal screenshot
- Menu options
- "Let's see it in action!"

#### Slide 35: Q&A
- Large "Q&A" text
- Common questions preview
- Contact info

#### Slide 36: Thank You
- Thank you message
- Contact links
- QR code to repo
- Call to action (Star the repo!)

---

## Design Recommendations

### Color Scheme
- **Primary**: #512BD4 (Microsoft Purple)
- **Secondary**: #0078D4 (Azure Blue)
- **Accent**: #107C10 (Success Green)
- **Code Background**: #1E1E1E (VS Dark)
- **Text**: #323130 (Gray 900)

### Fonts
- **Headings**: Segoe UI Semibold
- **Body**: Segoe UI Regular
- **Code**: Cascadia Code / Consolas

### Icons & Images
- Use .NET logo (download from Microsoft)
- Use EF Core logo
- Use SQL Server logo
- Font Awesome icons for bullets

### Code Blocks
- Dark theme (VS Code Dark+)
- Syntax highlighting
- Line numbers for important sections
- Keep code snippets short (10-15 lines max)

---

## Method 4: Google Slides

### Template Structure
1. Create from "Tech Pitch" template
2. Use two-column layout for code comparisons
3. Use "Code" theme for syntax highlighting

### Import Steps
1. Copy section by section from PRESENTATION.md
2. Format code with monospace font
3. Add speaker notes from SPEAKER_NOTES.md
4. Insert images/screenshots from demos

---

## Adding Visuals

### Recommended Screenshots

1. **Complex Types Demo**
   - Screenshot of SSMS showing table structure
   - Columns: ShippingAddress_Street, ShippingAddress_City
   - JSON column: BillingAddress

2. **ExecuteUpdate Demo**
   - Before: Loop with 1000 UPDATEs
   - After: Single UPDATE statement
   - Performance graph

3. **Vector Search Demo**
   - Console output showing similarity scores
   - Diagram: Text ? Embedding ? Vector
   - 3D visualization of vector space (optional)

4. **Named Filters Demo**
   - Console showing tenant switching
   - SQL with automatic WHERE clause highlighted

5. **LeftJoin Demo**
   - Side-by-side code comparison
   - Old vs New syntax

6. **Architecture Diagram**
   - Boxes: Models, Services, DemoStrategies, Data
   - Arrows showing flow
   - Strategy pattern UML (optional)

---

## Animations (PowerPoint)

### Recommended Animations

**Slide Entrance**:
- Fade in (0.5s duration)
- No sound

**Code Reveals**:
1. Show problem code
2. Pause
3. Show solution (Appear from bottom)
4. Highlight differences (Color change)

**Performance Numbers**:
- **Before**: Appear first
- **After**: Appear with sound (whoosh)
- **Speedup**: Emphasize (Grow/Shrink)

**Demo Results**:
- Typewriter effect for console output
- Line by line appearance

**Bullet Points**:
- Appear one at a time
- On click (not automatic)
- From left

---

## Presenter View Setup

### Notes for Each Slide
(Already in SPEAKER_NOTES.md)

### Timer Settings
- 60 minutes total
- Warning at 50 minutes
- Final warning at 55 minutes

### Rehearse Timings
```
Intro: 3 min
Complex Types: 8 min
ExecuteUpdate: 6 min
Vector Search: 12 min
Named Filters: 7 min
LeftJoin: 5 min
Collections: 4 min
Wrap-up: 10 min
Q&A: 10 min
```

---

## Print Handouts

### Recommended Format
- 3 slides per page
- Include notes section
- Black & white friendly
- QR code on last page

### Content to Include
- All code examples
- Performance tables
- Resource links
- GitHub repo link

---

## Recording the Presentation

### Tools
- **OBS Studio** (Free, open-source)
- **Zoom** (Built-in recording)
- **PowerPoint** (Record Slide Show)

### Settings
- 1920x1080 resolution
- 30 fps
- Screen + webcam (picture-in-picture)
- Clear audio (use external mic if possible)

### Post-Processing
- Add intro/outro cards
- Add chapter markers
- Upload to YouTube
- Share in repo README

---

## Interactive Elements

### Live Coding Setup
1. Open Visual Studio in second monitor
2. Have terminal ready with `dotnet run`
3. Prepare breakpoints in interesting code sections
4. Have SQL Server Management Studio open

### Polls/Quizzes
- "Who's using EF Core 7+?"
- "Who has used vector search before?"
- "What's your biggest EF Core pain point?"

### Code Challenge
> "Can you spot the performance problem in this query?"
```csharp
var orders = await context.Orders
    .Where(o => o.Status == "Pending")
    .ToListAsync();

foreach (var order in orders) {
    order.Status = "Shipped";
}
await context.SaveChangesAsync();
```
*Answer: Use ExecuteUpdate instead!*

---

## Accessibility

### Alt Text for Images
- All screenshots need descriptions
- Code snippets: "Code example showing..."
- Diagrams: Describe flow/relationships

### Color Contrast
- Ensure 4.5:1 ratio for text
- Don't rely on color alone
- Test with "Grayscale" mode

### Font Size
- Minimum 18pt for body text
- Minimum 24pt for headings
- Code: Minimum 14pt

---

## Export Checklist

- [ ] All code syntax highlighted
- [ ] All links working
- [ ] Speaker notes included
- [ ] Fonts embedded
- [ ] Images high-resolution
- [ ] Animations tested
- [ ] Presenter view configured
- [ ] Backup PDF created
- [ ] Handouts generated
- [ ] Demo code ready

---

## Distribution

### GitHub
- Upload PPTX to repo
- Include in README
- Tag as release

### SlideShare
- Upload to SlideShare
- Add tags: EF Core, .NET, Entity Framework
- Link in bio

### YouTube
- Record presentation
- Add timestamps
- Link slides in description

---

**Ready to Present!** ??
