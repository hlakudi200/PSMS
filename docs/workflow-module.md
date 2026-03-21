# Workflow Module Documentation

## Overview

The Workflow Module provides a **configurable, multi-tenant approval engine** for the PSMS platform. Each school (tenant) can define their own approval workflows with custom steps, roles, and routing logic. The module tracks every transition with a full audit trail.

**Key capabilities:**
- Define approval chains per entity type (Applications, Reports, Fee Waivers, etc.)
- Each tenant configures their own steps, order, and role assignments
- Role-based and user-specific step assignments
- SLA deadlines with overdue tracking
- Delegation for absent staff
- Batch approve/reject operations
- Full transition audit history
- Dashboard with stats and activity feed



## Accessing the Workflow Module

**URL:** `/admin/workflow`

**Allowed Roles:** Admin, Principal, Vice Principal

**Navigation:** Admin sidebar > Workflows

The module has four pages:

| Page | URL | Description |
|------|-----|-------------|
| Dashboard | `/admin/workflow` | Stats, summaries by entity type/role, recent activity |
| Definitions | `/admin/workflow/definitions` | Create and configure workflow templates |
| Instances | `/admin/workflow/instances` | Monitor and act on running workflows |
| Delegations | `/admin/workflow/delegations` | Manage delegation of workflow responsibilities |

---

## Concepts

### Workflow Definition
A reusable template that describes the steps an entity goes through for approval. Each tenant can have **one active definition per entity type** at a time.

### Workflow Step
A single stage in the approval chain. Each step defines:
- **Who** is responsible (role or specific user)
- **What action** they perform (Review, Approve, Reject, etc.)
- **What happens next** on approval or rejection
- Whether it's the **terminal** (final) step
- Optional **SLA deadline** (hours)
- Whether a **comment is required**

### Workflow Instance
A running workflow linked to a specific entity (e.g., a particular student application). Tracks current step, status, and all transitions.

### Workflow Transition
An immutable audit record capturing every state change: who acted, what action they took, from/to step, comment, and timestamp.

### Workflow Delegation
Allows a user to delegate their workflow responsibilities to another user during a date range (e.g., leave, conference). Can be scoped to specific entity types and roles.

---

## Entity Types

Workflows can be attached to these entity types:

| Value | Entity Type | Example Use Case |
|-------|-------------|-----------------|
| 1 | Application | Student admissions approval |
| 2 | Report | Report card approval before publishing |
| 3 | Fee Waiver | Fee discount approval |
| 4 | Attendance | Attendance dispute review |
| 5 | Learning Material | Content review before publishing |
| 6 | Student Transfer | Transfer request approval |
| 7 | Disciplinary | Disciplinary action approval |

---

## Workflow Statuses

| Status | Description |
|--------|-------------|
| Not Started | Instance created but not yet initiated |
| In Progress | Workflow is actively being processed |
| Completed | All steps approved, workflow finished successfully |
| Rejected | Workflow was rejected at a step |
| Cancelled | Workflow was cancelled by an authorized user |
| Recalled | Workflow was recalled by its creator |

---

## Action Types

| Action | Description |
|--------|-------------|
| Submit | Initial submission of the entity into the workflow |
| Review | Reviewer examines the entity (stays at or advances from step) |
| Approve | Approves the current step; advances to next or completes if terminal |
| Reject | Rejects at current step; routes to specified step or terminates |
| Revise | Sends back for revision (stays at current step) |
| Cancel | Cancels the entire workflow |
| Recall | Creator pulls back the workflow |

---

## How to Configure a Workflow

### Step 1: Create a Workflow Definition

1. Navigate to **Definitions** page
2. Click **New Definition**
3. Fill in:
   - **Name**: A descriptive name (e.g., "Admissions Approval")
   - **Entity Type**: Select which entity this workflow applies to
   - **Description**: Optional description
   - **Active**: Toggle on if you want this to be the active workflow immediately
4. Click **OK**

> **Important:** Only one definition can be active per entity type per tenant. Activating a new definition automatically deactivates the previous one.

### Step 2: Add Steps

1. Click **View Details** on your new definition
2. Click **Add Step**
3. Configure each step:

| Field | Description | Required |
|-------|-------------|----------|
| Step Name | Display name (e.g., "Principal Review") | Yes |
| Step Order | Sequence position (1, 2, 3...) | Yes |
| Assigned Role | Which role handles this step | Yes |
| Action Type | What action the assignee performs | Yes |
| Terminal Step | If ON, approving this step completes the workflow | No |
| Comment Required | Force the user to leave a comment | No |
| Next Step on Approve | Step order to go to on approval (default: next sequential) | No |
| Next Step on Reject | Step order to go to on rejection (default: reject workflow) | No |
| SLA Hours | Deadline in hours; instance flagged as overdue if exceeded | No |
| Assigned User ID | Assign to a specific user instead of a role | No |
| Guard Expression | Conditional expression for step entry (future use) | No |

4. Repeat for each step in the approval chain
5. Use the **arrow buttons** to reorder steps if needed

### Step 3: Activate the Definition

1. Go back to the Definitions list
2. Click the **Activate** action on your definition
3. The definition is now the active workflow for that entity type

---

## Example: Configuring an Admissions Workflow

A school wants this approval flow for student applications:

```
Application Submitted
    |
    v
[Step 1] Secretary reviews for completeness (Admin role, Review action)
    |
    v
[Step 2] HOD conducts interview (HOD role, Review action, comment required)
    |
    v
[Step 3] Principal makes final decision (Principal role, Approve action, TERMINAL)
    |                                          |
    v (Approve)                                v (Reject)
 COMPLETED                              Back to Step 2
```

**Configuration:**

1. Create definition: Name = "School Admissions", Entity Type = Application
2. Add steps:

| Order | Name | Role | Action | Terminal | Comment Req. | Next on Reject |
|-------|------|------|--------|----------|--------------|----------------|
| 1 | Secretary Review | Admin | Review | No | No | — |
| 2 | HOD Interview | HOD | Review | No | Yes | — |
| 3 | Principal Decision | Principal | Approve | Yes | Yes | 2 |

3. Activate the definition

Now when any application enters the workflow, it follows this school's custom approval chain.

---

## Default Workflows

The system provides three default workflows that can be seeded for new tenants:

### 1. Admissions Approval (5 steps)
| # | Step | Role | Action | Terminal | On Reject |
|---|------|------|--------|----------|-----------|
| 1 | Admin Review | Admin | Review | No | — |
| 2 | Document Verification | Admin | Review | No | — |
| 3 | Interview | Admin | Review | No | — |
| 4 | Committee Review | Admin | Review | No | — |
| 5 | Principal Decision | Principal | Approve | Yes | Step 4 |

### 2. Report Approval (2 steps)
| # | Step | Role | Action | Terminal | On Reject |
|---|------|------|--------|----------|-----------|
| 1 | HOD Review | Teacher | Review | No | — |
| 2 | Principal Approval | Principal | Approve | Yes | Step 1 |

### 3. Fee Waiver Approval (2 steps)
| # | Step | Role | Action | Terminal | On Reject |
|---|------|------|--------|----------|-----------|
| 1 | Finance Review | Admin | Review | No | — |
| 2 | Principal Approval | Principal | Approve | Yes | — |

> These defaults can be customized or replaced entirely by each school.

---

## Working with Workflow Instances

### Starting a Workflow

1. Navigate to **Instances** page
2. Click **Start Workflow**
3. Select the **Entity Type**
4. Select the **Entity** from the dropdown (loads available records)
5. Optionally select a specific **Workflow Definition** (defaults to the active one)
6. Click **OK**

The workflow starts at Step 1 and is assigned to the role/user configured for that step.

### Taking Action on a Workflow

1. Find the instance in the list (filter by status, entity type, or use the "My Role" filter)
2. Click **Take Action**
3. Select an action:
   - **Approve** — Advances to next step (or completes if terminal)
   - **Reject** — Routes to reject step (or terminates the workflow)
   - **Review** — Marks as reviewed (stays at current step)
   - **Send for Revision** — Requests changes (stays at current step)
4. Add a comment (required if step mandates it)
5. Click **OK**

### Quick Filters

The Instances page provides toolbar filters:
- **Overdue** — Show only instances past their SLA deadline
- **My Role** — Show only instances assigned to your current role

### Batch Operations

1. Select multiple instances using checkboxes
2. Use bulk actions:
   - **Batch Approve** — Approves all selected in-progress workflows
   - **Batch Reject** — Rejects all selected in-progress workflows

### Cancelling / Recalling

- **Cancel**: Authorized users (Admin, Principal) can cancel any in-progress workflow
- **Recall**: The workflow creator can recall their own workflow (pulls it back)

---

## Viewing Workflow History

1. Click **View Details** on any instance
2. The detail page shows:
   - Current status, step, assigned role, SLA due date
   - **Transition History** — A timeline of every action taken, showing:
     - Action type (color-coded)
     - From/to step
     - Who acted and when
     - Comments left

---

## Delegation

Delegation allows staff to transfer their workflow responsibilities temporarily.

### Creating a Delegation

1. Navigate to **Delegations** page
2. Click **New Delegation**
3. Fill in:
   - **Delegate User ID**: The user who will take over responsibilities
   - **Start Date / End Date**: When the delegation is active
   - **Reason**: Optional (e.g., "Annual leave", "Conference")
   - **Scope: Entity Type**: Limit to a specific workflow type (or leave empty for all)
   - **Scope: Role**: Limit to a specific role (or leave empty for all)
4. Click **OK**

### How Delegation Works

When a workflow step is assigned to a role or user:
1. System first checks if the step is assigned to a **specific user**
   - If that user has an active delegation, the delegate can also act
2. If assigned to a **role**, any user with that role can act
   - Additionally, anyone with an active delegation from a user with that role can act

### Revoking a Delegation

Click **Revoke** on an active delegation to immediately end it. Only the delegator or Admin can revoke.

---

## Dashboard

The workflow dashboard provides an at-a-glance view:

### Statistics Cards
- **Active Workflows** — Currently running (Not Started + In Progress)
- **Completed** — Successfully finished
- **Rejected** — Rejected at any step
- **Overdue** — Past SLA deadline
- **My Pending** — Workflows assigned to your role or delegated to you
- **Avg. Completion (hours)** — Average time to complete (last 90 days)

### Summary Tables
- **By Entity Type** — Breakdown of active/completed/rejected/overdue per type
- **By Role** — Pending and overdue counts per role

### Activity Feed
- Timeline of recent workflow transitions (clickable — navigates to instance detail)
- Color-coded by action type

---

## Permissions Reference

| Permission | Description |
|------------|-------------|
| `Workflow.Definitions.View` | View workflow definitions |
| `Workflow.Definitions.Create` | Create new definitions |
| `Workflow.Definitions.Edit` | Edit definition name/description |
| `Workflow.Definitions.Delete` | Delete definitions (no active instances) |
| `Workflow.Definitions.Activate` | Activate/deactivate definitions |
| `Workflow.Instances.View` | View workflow instances |
| `Workflow.Instances.ViewAll` | View all instances (not just own role) |
| `Workflow.Instances.Start` | Start new workflow instances |
| `Workflow.Instances.Advance` | Take action (approve/reject/review) |
| `Workflow.Instances.Cancel` | Cancel running workflows |
| `Workflow.Instances.Recall` | Recall own workflows |
| `Workflow.Instances.ViewHistory` | View transition history |
| `Workflow.Instances.BatchAdvance` | Batch approve/reject multiple instances |
| `Workflow.Delegations.View` | View delegations |
| `Workflow.Delegations.Create` | Create delegations |
| `Workflow.Delegations.Revoke` | Revoke delegations |

---

## Architecture Overview

### Backend Structure

```
psms.Core/
  Domain/Workflow/
    Entities/
      WorkflowDefinition.cs      # Template with steps
      WorkflowStep.cs            # Single step in approval chain
      WorkflowInstance.cs         # Running workflow for an entity
      WorkflowTransition.cs       # Audit record of each state change
      WorkflowDelegation.cs       # User-to-user delegation
    Enums/
      WorkflowEntityType.cs
      WorkflowStatus.cs
      WorkflowActionType.cs
  Authorization/
    PermissionNames.cs            # 27 workflow permissions

psms.Application/
  Workflow/
    WorkflowDefinitions/          # CRUD + activate/deactivate
    WorkflowSteps/                # CRUD + reorder
    WorkflowInstances/            # Start, advance, cancel, recall, batch
    WorkflowDelegations/          # CRUD + revoke
    Dashboard/                    # Stats + activity feed
    Shared/
      WorkflowMapper.cs           # AutoMapper profile
      WorkflowExceptionCodes.cs   # 37 error codes
      WorkflowEntityBridgeService.cs  # Bridges completion to domain entities
    Seed/
      WorkflowSeedService.cs      # Seeds default workflows for tenants
```

### Frontend Structure

```
providers/workflow/
  shared/interfaces.ts            # All TypeScript types + enum labels
  workflow-definitions/           # Provider (context, actions, reducer, index)
  workflow-steps/                 # Provider
  workflow-instances/             # Provider
  workflow-delegations/           # Provider
  workflow-dashboard/             # Provider

components/modals/workflow/
  WorkflowDefinitionFormModal.tsx # Create/edit definition
  WorkflowStepFormModal.tsx       # Create/edit step
  StartWorkflowModal.tsx          # Start workflow (entity picker)
  AdvanceWorkflowModal.tsx        # Take action with comment
  WorkflowDelegationFormModal.tsx # Create delegation

components/modules/workflow/
  WorkflowDashboardContent.tsx    # Dashboard page
  WorkflowDefinitionsPageContent.tsx  # Definitions list
  WorkflowDefinitionDetailPage.tsx    # Definition + step management
  WorkflowInstancesPageContent.tsx    # Instances list + filters
  WorkflowInstanceDetailPage.tsx      # Instance detail + history
  WorkflowDelegationsPageContent.tsx  # Delegations list

app/admin/workflow/
  layout.tsx                      # Sidebar layout
  page.tsx                        # Dashboard route
  definitions/page.tsx            # Definitions route
  definitions/[id]/page.tsx       # Definition detail route
  instances/page.tsx              # Instances route
  instances/[id]/page.tsx         # Instance detail route
  delegations/page.tsx            # Delegations route
```

### Multi-Tenancy

- Each tenant has their own workflow definitions and instances
- All entities use `IMayHaveTenant` with explicit `TenantId` filtering
- Steps without `IMayHaveTenant` are filtered via parent definition's tenant
- The `Abp-TenantId` header is sent automatically by the axios interceptor

### Bridge Service

When a workflow completes or is rejected, the `WorkflowEntityBridgeService` automatically updates the linked domain entity:

- **Application** approved → calls `application.Approve(userId)`
- **Application** rejected → calls `application.Reject(userId, reason)`
- **Report** approved → calls `report.Approve(userId)`
- **Report** rejected → resets report status to Generated

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| "No active definition" when starting workflow | No definition is activated for that entity type | Go to Definitions, activate one |
| "Active instance already exists" | Entity already has a running workflow | Complete, cancel, or recall the existing one first |
| "Step in use by instances" when deleting step | Active workflows reference this step | Complete or cancel those workflows first |
| "Definition has active instances" when deleting | Running workflows use this definition | Deactivate instead, or wait for completion |
| Delegation not working | Date range expired or wrong scope | Check start/end dates and entity type/role scope |
| User cannot take action on step | User doesn't have the required role or delegation | Assign correct role or create delegation |
| Overdue badge shows but no deadline set | SLA hours not configured on step | Edit the step and set SLA hours |
