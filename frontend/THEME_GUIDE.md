# PSMS Theme & Style Guide

## Overview
The PSMS frontend uses a **classic enterprise ERP aesthetic** inspired by Oracle E-Business Suite and SAP GUI systems. This provides a professional, utilitarian design optimized for information density and productivity.

## Design Philosophy
- **High Contrast**: Dark text on light backgrounds for maximum readability
- **Information Dense**: Compact spacing and smaller fonts to show more data
- **Minimal Decoration**: Sharp corners, thin borders, flat design
- **Professional Colors**: Business blue primary with standard success/warning/error colors
- **Grid-Based**: Traditional table-heavy layouts
- **No Motion**: Snappy interactions without animations

---

## Color Palette

### Primary Colors
- **Primary Blue**: `#0066CC` - Main action color, links, selected states
- **Success Green**: `#52c41a` - Positive actions, success states
- **Warning Amber**: `#faad14` - Warnings, pending states
- **Error Red**: `#ff4d4f` - Errors, destructive actions
- **Info Blue**: `#1890ff` - Informational messages

### Neutrals
- **Background**: `#F5F5F5` - Page background (light gray)
- **Container**: `#FFFFFF` - Cards, modals, forms
- **Layout**: `#E5E5E5` - Slightly darker layout areas
- **Borders**: `#D9D9D9` - Default borders
- **Text Primary**: `#262626` - Main text color
- **Text Secondary**: `#595959` - Labels, less important text
- **Text Tertiary**: `#8C8C8C` - Placeholders, hints

### Header/Nav Colors
- **Header Background**: `#003D73` - Dark blue Oracle-style header
- **Sidebar Background**: `#F0F0F0` - Light gray sidebar
- **Table Headers**: `#E8E8E8` - Grid-style headers

---

## Typography

### Font Stack
```css
font-family: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif;
```
Classic Windows system fonts for that enterprise feel.

### Sizes
- **Body**: 13px (denser than modern 14-16px)
- **H1**: 28px
- **H2**: 24px
- **H3**: 20px
- **H4**: 16px
- **H5**: 14px
- **H6**: 13px

---

## Spacing

### Padding/Margin Scale
- **XS**: 4px
- **SM**: 8px
- **Default**: 12px
- **LG**: 16px

Tighter spacing than modern designs for information density.

---

## Components

### Borders & Radius
- **Border Radius**: 2px (minimal, sharp corners)
- **Border Width**: 1px
- **Border Color**: `#D9D9D9`

### Buttons
- **Height**: 32px (standard), 40px (large), 24px (small)
- **Flat Design**: No shadows on buttons
- **Primary**: White text on `#0066CC` background
- **Default**: Dark text on white with gray border

### Tables
- **Header**: Gray background `#E8E8E8`
- **Row Hover**: `#F0F7FF` (light blue tint)
- **Selected Row**: `#E6F7FF` (slightly darker blue)
- **Cell Padding**: 8px vertical, 12px horizontal (compact)
- **Grid Lines**: `#D9D9D9` borders

### Forms
- **Input Height**: 32px
- **Label**: 13px, `#262626`, weight 500
- **Focus Border**: `#0066CC` with subtle shadow
- **Grid Layout**: Two-column by default

### Cards
- **Header**: `#FAFAFA` background
- **Border**: 1px solid `#D9D9D9`
- **Padding**: 16px
- **Minimal Shadow**: Subtle elevation only

---

## Layout Patterns

### Page Header
```jsx
<div className="psms-page-header">
  <h1>Page Title</h1>
</div>
```
White background with blue bottom border.

### Toolbar
```jsx
<div className="psms-toolbar">
  <Button>Action 1</Button>
  <Button>Action 2</Button>
</div>
```
Light gray background, horizontal button group.

### Form Grid
```jsx
<div className="psms-form-grid">
  <Form.Item>...</Form.Item>  {/* Left column */}
  <Form.Item>...</Form.Item>  {/* Right column */}
  <Form.Item className="psms-form-full">...</Form.Item>  {/* Full width */}
</div>
```
Two-column grid, collapses to single column on mobile.

### Content Area
```jsx
<div className="psms-content">
  <div className="psms-content-card">
    {/* Your content */}
  </div>
</div>
```

---

## Status Indicators

### Status Badges
```jsx
<Tag className="psms-status psms-status-success">Active</Tag>
<Tag className="psms-status psms-status-warning">Pending</Tag>
<Tag className="psms-status psms-status-error">Rejected</Tag>
<Tag className="psms-status psms-status-info">In Review</Tag>
<Tag className="psms-status psms-status-default">Inactive</Tag>
```

### Role Colors
```typescript
import { roleColors } from '@/utils/theme-config';

// roleColors.Admin → "#722ED1"
// roleColors.Principal → "#0066CC"
// roleColors.Teacher → "#2F54EB"
// ... etc
```

### Module Colors
```typescript
import { moduleColors } from '@/utils/theme-config';

// moduleColors.Academic → "#0066CC"
// moduleColors.Admissions → "#52C41A"
// moduleColors.Financial → "#FAAD14"
// ... etc
```

---

## Utility Classes

### Layout
- `.flex` - Display flex
- `.flex-col` - Flex column direction
- `.items-center` - Align items center
- `.justify-center` - Justify content center
- `.justify-between` - Justify space between
- `.w-full` - Full width
- `.h-full` - Full height

### Spacing
- `.gap-1` - 4px gap
- `.gap-2` - 8px gap
- `.gap-3` - 12px gap
- `.gap-4` - 16px gap

### Text Alignment
- `.text-center` - Center text
- `.text-left` - Left align text
- `.text-right` - Right align text

### States
- `.psms-loading` - Loading spinner container
- `.psms-error` - Error message container
- `.psms-empty` - Empty state container

---

## Table Styling

### Dense Tables
```jsx
<Table
  size="small"
  className="psms-table-sticky"
  pagination={{ pageSize: 50 }}
  rowSelection={...}
/>
```

### Action Buttons in Tables
```jsx
<div className="psms-table-actions">
  <button onClick={handleEdit}>Edit</button>
  <button onClick={handleDelete}>Delete</button>
</div>
```

---

## Scrollbars

Classic Windows-style scrollbars:
- **Track**: `#F0F0F0` with `#D9D9D9` border
- **Thumb**: `#C0C0C0` with darker border
- **Width**: 16px (traditional desktop size)

---

## Responsive Behavior

### Breakpoints (Ant Design)
- **xs**: < 576px
- **sm**: ≥ 576px
- **md**: ≥ 768px
- **lg**: ≥ 992px
- **xl**: ≥ 1200px
- **xxl**: ≥ 1600px

### Mobile Adjustments
- Form grids collapse to single column
- Table font size reduces to 12px
- Body font increases to 14px for readability

---

## Print Styles
- Background colors removed
- `.no-print` elements hidden
- Tables split properly across pages
- Headers repeat on each page

---

## Best Practices

### DO:
- Use the form grid for two-column layouts
- Apply status classes for visual indicators
- Keep information dense (don't over-space)
- Use sharp corners and minimal shadows
- Leverage the toolbar pattern for action groups
- Use table hover/selection for clear feedback

### DON'T:
- Use rounded corners > 4px
- Add animations or transitions
- Use gradient backgrounds
- Over-pad content (remember: information density)
- Use soft shadows or depth effects
- Apply modern "card" aesthetics

---

## Component Examples

### Login Form
```tsx
<Form layout="vertical" className="psms-compact">
  <Form.Item label="Username" name="username">
    <Input />
  </Form.Item>
  <Form.Item label="Password" name="password">
    <Input.Password />
  </Form.Item>
  <Form.Item>
    <Button type="primary" htmlType="submit" block>
      Log In
    </Button>
  </Form.Item>
</Form>
```

### Data Table with Actions
```tsx
<div className="psms-content-card">
  <div className="psms-toolbar">
    <Button type="primary">New Student</Button>
    <Button>Import</Button>
    <Button>Export</Button>
  </div>
  <Table
    size="small"
    dataSource={students}
    columns={[
      { title: 'ID', dataIndex: 'admissionNumber' },
      { title: 'Name', dataIndex: 'fullName' },
      { title: 'Grade', dataIndex: 'gradeName' },
      {
        title: 'Actions',
        render: (_, record) => (
          <div className="psms-table-actions">
            <button onClick={() => handleEdit(record)}>Edit</button>
            <button onClick={() => handleView(record)}>View</button>
          </div>
        ),
      },
    ]}
  />
</div>
```

### Dashboard Card
```tsx
<Card
  title="Student Enrollment"
  extra={<Button size="small">View All</Button>}
>
  <Statistic title="Total Students" value={1234} />
  <Statistic title="New This Term" value={45} />
</Card>
```

---

## Accessibility

- Keyboard focus visible via `outline: 2px solid #0066CC`
- High contrast ratios for text
- Focus indicators on all interactive elements
- Semantic HTML maintained
- ARIA labels where appropriate

---

## Configuration Files

### Theme Config
`src/utils/theme-config.ts` - Complete Ant Design theme object

### Global CSS
`src/app/globals.css` - Global styles and utility classes

### Provider Setup
`src/app/providers.tsx` - ConfigProvider with theme

---

## Future Enhancements

If needed, consider adding:
- Dark mode toggle (with enterprise dark theme)
- Print-optimized layout templates
- High-contrast mode for accessibility
- Additional color themes per tenant
