#!/usr/bin/env node
/**
 * Seeds one realistic South African school (an ABP tenant) through the public API.
 *
 * Idempotent: every step looks for what it needs before creating it, so a re-run
 * after a failure resumes rather than duplicating. Natural keys are the tenancy
 * name, the academic year, grade level, subject code, employee number, admission
 * number and user name.
 *
 * Credentials come from the environment only — nothing is written to the repo.
 *
 *   PSMS_API_URL                base URL (default: https://psms-6tb7.onrender.com)
 *   PSMS_HOST_ADMIN             host admin user name (default: admin)
 *   PSMS_HOST_PASSWORD          host admin password (required)
 *   PSMS_SCHOOL_TENANCY         tenancy name (default: riverside)
 *   PSMS_SCHOOL_NAME            display name (default: Riverside College)
 *   PSMS_SCHOOL_ADMIN_EMAIL     tenant admin email (default: admin@riverside.test)
 *   PSMS_TENANT_ADMIN_PASSWORD  tenant admin password (default: ABP's 123qwe)
 *
 * Usage:
 *   PSMS_HOST_PASSWORD=... node scripts/seed-school.mjs            # seed
 *   PSMS_HOST_PASSWORD=... node scripts/seed-school.mjs --dry-run  # plan only
 */

const API = (process.env.PSMS_API_URL ?? 'https://psms-6tb7.onrender.com').replace(/\/$/, '');
const HOST_ADMIN = process.env.PSMS_HOST_ADMIN ?? 'admin';
const HOST_PASSWORD = process.env.PSMS_HOST_PASSWORD;
const TENANCY = process.env.PSMS_SCHOOL_TENANCY ?? 'riverside';
const SCHOOL_NAME = process.env.PSMS_SCHOOL_NAME ?? 'Riverside College';
const SCHOOL_ADMIN_EMAIL = process.env.PSMS_SCHOOL_ADMIN_EMAIL ?? `admin@${TENANCY}.test`;
const TENANT_PASSWORD = process.env.PSMS_TENANT_ADMIN_PASSWORD ?? '123qwe';
const DRY_RUN = process.argv.includes('--dry-run');

// Backend enums (psms.Domain.Shared.Enums)
const GradeLevel = { GradeR: 0, Grade1: 1, Grade2: 2, Grade3: 3, Grade4: 4, Grade5: 5, Grade6: 6, Grade7: 7 };
const Phase = { Foundation: 1, Intermediate: 2, Senior: 3 };
const Gender = { Male: 1, Female: 2 };
const FeeType = { Tuition: 1, Registration: 2 };
const Relationship = { Father: 1, Mother: 2 };

// ---------------------------------------------------------------- the school

const YEAR = new Date().getFullYear();

const GRADES = [
  { level: GradeLevel.GradeR, name: 'Grade R', phase: Phase.Foundation, classes: 1, tuition: 18000 },
  { level: GradeLevel.Grade1, name: 'Grade 1', phase: Phase.Foundation, classes: 2, tuition: 24000 },
  { level: GradeLevel.Grade2, name: 'Grade 2', phase: Phase.Foundation, classes: 1, tuition: 24000 },
  { level: GradeLevel.Grade3, name: 'Grade 3', phase: Phase.Foundation, classes: 1, tuition: 24000 },
  { level: GradeLevel.Grade4, name: 'Grade 4', phase: Phase.Intermediate, classes: 2, tuition: 28500 },
  { level: GradeLevel.Grade5, name: 'Grade 5', phase: Phase.Intermediate, classes: 1, tuition: 28500 },
  { level: GradeLevel.Grade6, name: 'Grade 6', phase: Phase.Intermediate, classes: 1, tuition: 28500 },
  { level: GradeLevel.Grade7, name: 'Grade 7', phase: Phase.Senior, classes: 1, tuition: 32000 },
];

const SUBJECTS = [
  { subjectCode: 'ENGHL', subjectName: 'English Home Language', isCore: true },
  { subjectCode: 'AFRFAL', subjectName: 'Afrikaans First Additional Language', isCore: true },
  { subjectCode: 'ISIZUL', subjectName: 'isiZulu First Additional Language', isCore: false },
  { subjectCode: 'MATH', subjectName: 'Mathematics', isCore: true },
  { subjectCode: 'NST', subjectName: 'Natural Sciences and Technology', isCore: true },
  { subjectCode: 'SS', subjectName: 'Social Sciences', isCore: true },
  { subjectCode: 'LIFSK', subjectName: 'Life Skills', isCore: true },
  { subjectCode: 'LO', subjectName: 'Life Orientation', isCore: true },
  { subjectCode: 'CAT', subjectName: 'Computer Applications Technology', isCore: false },
  { subjectCode: 'CREART', subjectName: 'Creative Arts', isCore: false },
  { subjectCode: 'PE', subjectName: 'Physical Education', isCore: false },
  { subjectCode: 'ECMAN', subjectName: 'Economic and Management Sciences', isCore: false },
];

const FIRST_NAMES = [
  'Thabo', 'Lerato', 'Sipho', 'Naledi', 'Kagiso', 'Amahle', 'Bongani', 'Zanele', 'Tshepo', 'Palesa',
  'Mandla', 'Nomsa', 'Katlego', 'Refilwe', 'Sibusiso', 'Thandeka', 'Lungile', 'Ayanda', 'Mpho', 'Nandi',
  'Johan', 'Marieke', 'Pieter', 'Annelie', 'Riaan', 'Chantal', 'Ruan', 'Elmarie', 'Dewald', 'Suzanne',
  'Aarav', 'Priya', 'Rohan', 'Divya', 'Yusuf', 'Fatima', 'Imran', 'Zainab', 'Daniel', 'Kayla',
];

const SURNAMES = [
  'Dlamini', 'Nkosi', 'Mokoena', 'Khumalo', 'Ndlovu', 'Mahlangu', 'Sithole', 'Zulu', 'Mabaso', 'Molefe',
  'van der Merwe', 'Botha', 'Pretorius', 'Van Wyk', 'Du Plessis', 'Fourie', 'Nel', 'Steyn',
  'Naidoo', 'Pillay', 'Govender', 'Reddy', 'Patel', 'Cassim',
  'Adams', 'September', 'Arendse', 'Fortuin', 'Daniels', 'Jacobs',
];

const TEACHER_COUNT = 15;
const PARENT_COUNT = 100;
const STUDENT_COUNT = 150;

// The school day the generated timetable is built on.
const PERIODS_PER_DAY = 8;
const PERIOD_START = '08:00';
const PERIOD_MINUTES = 40;
const BREAK_AFTER_PERIODS = [3, 6];   // first and second break
const BREAK_MINUTES = 20;

const ATTENDANCE_DAYS = 10;   // school days back from today
const PAID_FEE_RATIO = 0.65;  // share of tuition fees that already have a payment

const ANNOUNCEMENTS = [
  { title: 'Term parent evening', type: 4, priority: 3, targetAudience: 4,
    content: 'Parent evening is on the 14th at 18:00 in the school hall. Each grade head presents the term plan, and class teachers are available afterwards for one-on-one conversations.' },
  { title: 'Winter sports trials', type: 3, priority: 2, targetAudience: 5,
    content: 'Trials for netball, rugby and hockey run all of next week on the top fields. Bring your own boots and a water bottle. Sign-up sheets are outside the sports office.' },
  { title: 'Load-shedding contingency', type: 5, priority: 4, targetAudience: 1,
    content: 'When stage 4 or higher is scheduled during school hours the generator covers the academic block only. Aftercare continues as normal and collection times do not change.' },
  { title: 'Stationery packs for next term', type: 7, priority: 2, targetAudience: 4,
    content: 'Stationery packs can be ordered through the school shop until the end of the month. Orders placed after that date will only be ready in the second week of next term.' },
  { title: 'Grade 7 leadership camp', type: 4, priority: 3, targetAudience: 1,
    content: 'The Grade 7 leadership camp runs from Wednesday to Friday. Indemnity forms must reach the class teacher before the end of this week.' },
  { title: 'Staff development day', type: 7, priority: 2, targetAudience: 1,
    content: 'The school is closed to learners on the first Friday of next month for staff development. Aftercare is not available that day.' },
];

const EXTRAMURALS = [
  { activityName: 'Netball', category: 1, activityType: 1, venue: 'Top courts', feePerTerm: 350, coachName: 'Ms Dlamini' },
  { activityName: 'Rugby', category: 1, activityType: 1, venue: 'Main field', feePerTerm: 400, coachName: 'Mr Botha' },
  { activityName: 'Chess club', category: 3, activityType: 2, venue: 'Library', feePerTerm: 150, coachName: 'Mr Naidoo' },
  { activityName: 'Marimba band', category: 2, activityType: 3, venue: 'Music room', feePerTerm: 450, coachName: 'Ms Khumalo' },
  { activityName: 'Athletics', category: 1, activityType: 2, venue: 'Track', feePerTerm: 300, coachName: 'Mr September' },
];

const TRANSPORT_ROUTES = [
  { routeName: 'Route A — Northern suburbs', transportType: 1, capacity: 45, monthlyFee: 950,
    vehicleNumber: 'CA 143-221', driverName: 'Mr Mokoena', areasCovered: 'Parkview, Greenside, Emmarentia' },
  { routeName: 'Route B — Southern suburbs', transportType: 2, capacity: 22, monthlyFee: 1100,
    vehicleNumber: 'CA 887-004', driverName: 'Mr Adams', areasCovered: 'Rondebosch, Newlands, Claremont' },
  { routeName: 'Route C — East', transportType: 3, capacity: 15, monthlyFee: 1250,
    vehicleNumber: 'CA 552-119', driverName: 'Ms Pillay', areasCovered: 'Observatory, Salt River' },
];

const DISCIPLINARY_CASES = [
  { incidentCategory: 6, severity: 1, location: 'Grade block',
    incidentDescription: 'Arrived more than twenty minutes late to first period on three consecutive days without a note from home.' },
  { incidentCategory: 9, severity: 1, location: 'Assembly',
    incidentDescription: 'Out of uniform for the second time this month, wearing non-regulation shoes with the summer uniform.' },
  { incidentCategory: 1, severity: 2, location: 'Classroom 4B',
    incidentDescription: 'Disrupted a Mathematics lesson repeatedly after two warnings, then left the classroom without permission.' },
  { incidentCategory: 5, severity: 2, location: 'Main corridor',
    incidentDescription: 'Broke a corridor window while kicking a ball indoors. Nobody was hurt and the learner reported it himself.' },
  { incidentCategory: 2, severity: 3, location: 'Playground',
    incidentDescription: 'Persistent name-calling directed at a younger learner over two weeks, reported by a playground supervisor.' },
];

const LEAVE_REQUESTS = [
  { leaveType: 2, days: 2, reason: 'Down with flu, signed off by my doctor for two days.' },
  { leaveType: 1, days: 5, reason: 'Family holiday booked before the term dates were published.' },
  { leaveType: 3, days: 1, reason: 'Taking my mother to a specialist appointment in town.' },
  { leaveType: 6, days: 3, reason: 'Sitting the final examinations for my honours degree.' },
];

const FIELD_TRIPS = [
  { tripName: 'Science centre visit', destination: 'Cape Town Science Centre, Observatory',
    estimatedCost: 8500, numberOfStudents: 32, numberOfChaperones: 3,
    transportArrangement: 'Two school buses, leaving 07:30 and back by 14:00.',
    riskAssessmentNotes: 'Indoor venue with supervised exhibits. One adult to eleven learners. The lead teacher carries the first-aid kit.',
    emergencyPlan: 'Lead teacher holds the class list and parent contacts. Groote Schuur is eight minutes away.' },
  { tripName: 'District athletics meet', destination: 'Green Point Athletics Stadium',
    estimatedCost: 4200, numberOfStudents: 18, numberOfChaperones: 2,
    transportArrangement: 'One minibus, full day.',
    riskAssessmentNotes: 'Outdoor summer event. Learners carry water and sunscreen; shaded seating is reserved for the school.',
    emergencyPlan: 'Event medics on site. The coach carries indemnity forms and emergency contacts.' },
  { tripName: 'Grade 7 leadership camp', destination: 'Hawequa Camp, Wellington',
    estimatedCost: 46000, numberOfStudents: 40, numberOfChaperones: 5,
    transportArrangement: 'Chartered coach, three nights away.',
    riskAssessmentNotes: 'Overnight camp with a river crossing and an obstacle course. Camp staff are accredited and a qualified lifeguard supervises all water activities.',
    emergencyPlan: 'Two staff sleep on site. Wellington Medi-Clinic is fifteen minutes away and the camp keeps a vehicle for emergencies.' },
];

const EXPENSES = [
  { category: 1, priority: 2, amount: 12400, vendor: 'Waltons', department: 'Foundation Phase',
    description: 'Replacement stationery for the Foundation Phase classrooms for the coming term.' },
  { category: 5, priority: 3, amount: 18750, vendor: 'Incredible Connection', department: 'IT',
    description: 'Ten replacement laptop chargers and two projector lamps for the computer room.' },
  { category: 4, priority: 4, amount: 34200, vendor: 'Cape Roofing Solutions', department: 'Facilities',
    description: 'Emergency repair to the leaking roof above classroom 6A before the winter rain.' },
  { category: 6, priority: 2, amount: 9600, vendor: 'Sportsmans Warehouse', department: 'Sport',
    description: 'New netball posts and match balls for the winter season.' },
];

const WAIVERS = [
  { waiverType: 1, share: 0.5, reason: 'Both parents were retrenched last quarter. The family has asked for relief on this year’s tuition while they look for work.' },
  { waiverType: 2, share: 0.15, reason: 'Third sibling enrolled at the school this year, requesting the standard sibling discount.' },
  { waiverType: 4, share: 1, reason: 'Learner placed in the top decile of the entrance assessment, requesting the academic bursary as advertised.' },
];

// ------------------------------------------------------------------ plumbing

/** Deterministic PRNG so a re-run generates the same people. */
let seedState = 20260914;
const rand = () => {
  seedState = (seedState * 1103515245 + 12345) & 0x7fffffff;
  return seedState / 0x7fffffff;
};
const pick = (arr) => arr[Math.floor(rand() * arr.length)];
const pad = (n, width) => String(n).padStart(width, '0');
const iso = (d) => d.toISOString().slice(0, 10);

const state = { token: null, tenantId: null };
let created = 0;
let reused = 0;

async function call(path, { method = 'POST', body, auth = true, tenant = true } = {}) {
  const headers = { 'Content-Type': 'application/json' };
  if (auth && state.token) headers.Authorization = `Bearer ${state.token}`;
  if (tenant && state.tenantId != null) headers['Abp-TenantId'] = String(state.tenantId);

  const res = await fetch(`${API}${path}`, {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  const text = await res.text();
  let payload;
  try {
    payload = text ? JSON.parse(text) : {};
  } catch {
    throw new Error(`${path} -> ${res.status}: ${text.slice(0, 300)}`);
  }

  if (!res.ok || payload.success === false) {
    const err = payload.error ?? {};
    const validation = (err.validationErrors ?? [])
      .map((v) => `${v.members?.join(',') ?? '?'}: ${v.message}`)
      .join('; ');
    throw new Error(
      `${path} -> ${res.status} ${err.message ?? 'request failed'}` +
      `${err.details ? ` (${err.details})` : ''}${validation ? ` [${validation}]` : ''}`
    );
  }
  return payload.result;
}

const get = (path) => call(path, { method: 'GET' });

/** Runs tasks with bounded concurrency — polite to a small dyno, still quick. */
async function mapPool(items, limit, fn) {
  const out = new Array(items.length);
  let cursor = 0;
  await Promise.all(
    Array.from({ length: Math.min(limit, items.length) }, async () => {
      while (cursor < items.length) {
        const i = cursor++;
        out[i] = await fn(items[i], i);
      }
    })
  );
  return out;
}

/** Create only when the natural key is absent. Returns the record either way. */
async function ensure(label, existing, createFn) {
  if (existing) {
    reused++;
    return existing;
  }
  if (DRY_RUN) {
    created++;
    return { id: `dry-${label}-${created}`, __dry: true };
  }
  const record = await createFn();
  created++;
  return record;
}

/**
 * Runs an optional section. The academic core (steps 1-12) must succeed, but the
 * colour on top is per-module: if one endpoint is missing or rejects, report it
 * and keep going rather than losing the rest of the run.
 */
async function soft(label, fn) {
  try {
    await fn();
  } catch (e) {
    console.log(`    skipped ${label}: ${e.message}`);
  }
}

const listAll = async (service, extra = '') =>
  (await get(`/api/services/app/${service}/GetAll?MaxResultCount=1000${extra}`))?.items ?? [];

function step(n, text) {
  console.log(`\n[${n}] ${text}`);
}

// ---------------------------------------------------------------------- seed

async function main() {
  if (!HOST_PASSWORD) {
    console.error('PSMS_HOST_PASSWORD is not set. Export the host admin password and re-run.');
    process.exit(1);
  }

  console.log(`PSMS school seeder`);
  console.log(`  API     ${API}`);
  console.log(`  school  ${SCHOOL_NAME} (${TENANCY})`);
  console.log(`  mode    ${DRY_RUN ? 'DRY RUN — nothing will be written' : 'live'}`);

  // 1. Host admin ------------------------------------------------------------
  step(1, 'Authenticating as host admin');
  state.tenantId = null;
  const hostAuth = await call('/api/TokenAuth/Authenticate', {
    body: { userNameOrEmailAddress: HOST_ADMIN, password: HOST_PASSWORD, rememberClient: false },
    auth: false,
    tenant: false,
  });
  state.token = hostAuth.accessToken;
  console.log('    ok');

  // 2. Tenant ----------------------------------------------------------------
  step(2, `Ensuring tenant "${TENANCY}"`);
  const tenants = (await get('/api/services/app/Tenant/GetAll?MaxResultCount=1000'))?.items ?? [];
  let tenant = tenants.find((t) => t.tenancyName?.toLowerCase() === TENANCY.toLowerCase());
  if (tenant) {
    console.log(`    already exists (id ${tenant.id}) — reusing`);
    reused++;
  } else if (DRY_RUN) {
    console.log('    would create');
    console.log('\nDry run stops here: everything after this needs the real tenant.');
    return;
  } else {
    tenant = await call('/api/services/app/Tenant/Create', {
      body: {
        tenancyName: TENANCY,
        name: SCHOOL_NAME,
        adminEmailAddress: SCHOOL_ADMIN_EMAIL,
        isActive: true,
      },
    });
    created++;
    console.log(`    created (id ${tenant.id})`);
  }

  // Backfill the operations permission groups (idempotent; safe on a fresh tenant).
  try {
    await call('/api/services/app/Tenant/GrantOperationsPermissions', { body: { id: tenant.id } });
    console.log('    operations permissions granted');
  } catch (e) {
    console.log(`    operations permissions skipped: ${e.message}`);
  }

  // 3. Tenant admin ----------------------------------------------------------
  step(3, 'Authenticating as the school admin');
  state.tenantId = tenant.id;
  state.token = null;
  const tenantAuth = await call('/api/TokenAuth/Authenticate', {
    body: { userNameOrEmailAddress: 'admin', password: TENANT_PASSWORD, rememberClient: false },
    auth: false,
  });
  state.token = tenantAuth.accessToken;
  console.log('    ok');

  // 4. Academic year ---------------------------------------------------------
  step(4, `Ensuring academic year ${YEAR} with its four terms`);
  const years = await listAll('AcademicYear');
  const year = await ensure(
    'year',
    years.find((y) => y.year === YEAR),
    () => call('/api/services/app/AcademicYear/Create', {
      body: {
        year: YEAR,
        startDate: `${YEAR}-01-15`,
        endDate: `${YEAR}-12-09`,
        createDefaultTerms: true,
      },
    })
  );
  console.log(`    ${year.yearName ?? YEAR}`);

  // 5. Grades ----------------------------------------------------------------
  step(5, `Ensuring ${GRADES.length} grades`);
  const existingGrades = await listAll('Grade');
  const grades = [];
  for (const g of GRADES) {
    const record = await ensure(
      'grade',
      existingGrades.find((x) => x.gradeLevel === g.level),
      () => call('/api/services/app/Grade/Create', {
        body: {
          gradeLevel: g.level,
          gradeName: g.name,
          schoolPhase: g.phase,
          description: `${g.name} — ${SCHOOL_NAME}`,
        },
      })
    );
    grades.push({ ...g, id: record.id });
  }
  console.log(`    ${grades.map((g) => g.name).join(', ')}`);

  // 6. Subjects --------------------------------------------------------------
  step(6, `Ensuring ${SUBJECTS.length} subjects`);
  const existingSubjects = await listAll('Subject');
  const subjects = [];
  for (const s of SUBJECTS) {
    const record = await ensure(
      'subject',
      existingSubjects.find((x) => x.subjectCode === s.subjectCode),
      () => call('/api/services/app/Subject/Create', { body: { ...s, description: s.subjectName } })
    );
    subjects.push({ ...s, id: record.id });
  }
  console.log(`    ${subjects.length} subjects`);

  // 7. Teachers --------------------------------------------------------------
  step(7, `Ensuring ${TEACHER_COUNT} teachers (user account + teacher record)`);
  const existingUsers = await listAll('User');
  const existingTeachers = await listAll('Teacher');
  const teachers = [];
  for (let i = 1; i <= TEACHER_COUNT; i++) {
    const employeeNumber = `EMP-${pad(i, 4)}`;
    const found = existingTeachers.find((t) => t.employeeNumber === employeeNumber);
    if (found) {
      teachers.push({ id: found.id, name: found.fullName ?? employeeNumber });
      reused++;
      continue;
    }

    const firstName = pick(FIRST_NAMES);
    const lastName = pick(SURNAMES);
    const userName = `teacher${pad(i, 3)}`;
    const email = `${userName}@${TENANCY}.test`;

    const user = await ensure(
      'teacher-user',
      existingUsers.find((u) => u.userName === userName),
      () => call('/api/services/app/User/Create', {
        body: {
          userName,
          name: firstName,
          surname: lastName,
          emailAddress: email,
          password: TENANT_PASSWORD,
          isActive: true,
          roleNames: ['Teacher'],
        },
      })
    );

    const teacher = await call('/api/services/app/Teacher/Create', {
      body: {
        userId: user.id,
        firstName,
        lastName,
        employeeNumber,
        email,
        phone: `08${Math.floor(rand() * 9)} ${pad(Math.floor(rand() * 1000), 3)} ${pad(Math.floor(rand() * 10000), 4)}`,
        dateOfJoining: `${YEAR - Math.floor(rand() * 8)}-01-15`,
        employmentStatus: 'Permanent',
        qualifications: 'BEd (Education)',
      },
    });
    created++;
    teachers.push({ id: teacher.id, name: `${firstName} ${lastName}` });
    process.stdout.write(`    ${teachers.length}/${TEACHER_COUNT}\r`);
  }
  console.log(`    ${teachers.length} teachers ready        `);

  // 8. Classes ---------------------------------------------------------------
  step(8, 'Ensuring classes');
  const existingClasses = await listAll('Class');
  const classes = [];
  let teacherCursor = 0;
  for (const grade of grades) {
    for (let c = 0; c < grade.classes; c++) {
      const className = grade.classes === 1 ? grade.name : `${grade.name}${String.fromCharCode(65 + c)}`;
      const classTeacher = teachers[teacherCursor++ % teachers.length];
      const record = await ensure(
        'class',
        existingClasses.find((x) => x.className === className),
        () => call('/api/services/app/Class/Create', {
          body: {
            className,
            gradeId: grade.id,
            academicYearId: year.id,
            maxCapacity: 32,
            classTeacherId: classTeacher.id,
          },
        })
      );
      classes.push({ id: record.id, className, gradeId: grade.id, grade });
    }
  }
  console.log(`    ${classes.map((c) => c.className).join(', ')}`);

  // 9. Class subjects --------------------------------------------------------
  step(9, 'Linking subjects to classes');
  const coreSubjects = subjects.filter((s) => s.isCore);
  let linkCursor = 0;
  let subjectLinks = 0;
  for (const cls of classes) {
    const existingLinks = await get(`/api/services/app/ClassSubject/GetByClass?classId=${cls.id}`) ?? [];
    const linked = new Set((existingLinks.items ?? existingLinks).map((l) => l.subjectId));
    for (const subject of coreSubjects) {
      if (linked.has(subject.id)) { reused++; continue; }
      await call('/api/services/app/ClassSubject/Create', {
        body: {
          classId: cls.id,
          subjectId: subject.id,
          teacherId: teachers[linkCursor++ % teachers.length].id,
          periodsPerWeek: subject.subjectCode === 'MATH' || subject.subjectCode === 'ENGHL' ? 5 : 3,
        },
      });
      created++;
      subjectLinks++;
    }
    process.stdout.write(`    ${subjectLinks} links\r`);
  }
  console.log(`    ${subjectLinks} class-subject links created        `);

  // 10. Parents --------------------------------------------------------------
  step(10, `Ensuring ${PARENT_COUNT} parents`);
  const existingParents = await listAll('Parent');
  const usersAfterTeachers = await listAll('User');
  const parents = [];
  for (let i = 1; i <= PARENT_COUNT; i++) {
    const userName = `parent${pad(i, 3)}`;
    const email = `${userName}@${TENANCY}.test`;
    const found = existingParents.find((p) => p.email === email);
    if (found) { parents.push({ id: found.id }); reused++; continue; }

    const firstName = pick(FIRST_NAMES);
    const lastName = pick(SURNAMES);
    const user = await ensure(
      'parent-user',
      usersAfterTeachers.find((u) => u.userName === userName),
      () => call('/api/services/app/User/Create', {
        body: {
          userName,
          name: firstName,
          surname: lastName,
          emailAddress: email,
          password: TENANT_PASSWORD,
          isActive: true,
          roleNames: ['Parent'],
        },
      })
    );

    const parent = await call('/api/services/app/Parent/Create', {
      body: {
        userId: user.id,
        firstName,
        lastName,
        email,
        phone: `08${Math.floor(rand() * 9)} ${pad(Math.floor(rand() * 1000), 3)} ${pad(Math.floor(rand() * 10000), 4)}`,
        occupation: pick(['Accountant', 'Nurse', 'Engineer', 'Teacher', 'Electrician', 'Small business owner']),
      },
    });
    created++;
    parents.push({ id: parent.id, lastName });
    process.stdout.write(`    ${parents.length}/${PARENT_COUNT}\r`);
  }
  console.log(`    ${parents.length} parents ready        `);

  // 11. Students -------------------------------------------------------------
  step(11, `Ensuring ${STUDENT_COUNT} students, spread across the classes`);
  const existingStudents = await listAll('Student');
  let studentsMade = 0;
  for (let i = 1; i <= STUDENT_COUNT; i++) {
    const admissionNumber = `${YEAR}-${pad(i, 4)}`;
    if (existingStudents.find((s) => s.admissionNumber === admissionNumber)) { reused++; continue; }

    const cls = classes[i % classes.length];
    const gradeOffset = Math.max(0, cls.grade.level);
    const firstName = pick(FIRST_NAMES);
    const parent = parents[i % parents.length];
    const lastName = parent.lastName ?? pick(SURNAMES);
    // Age lines up with the grade: Grade R turns 6, each grade a year older.
    const birthYear = YEAR - (6 + gradeOffset);

    const student = await call('/api/services/app/Student/Create', {
      body: {
        firstName,
        lastName,
        dateOfBirth: iso(new Date(Date.UTC(birthYear, Math.floor(rand() * 12), 1 + Math.floor(rand() * 27)))),
        gender: rand() > 0.5 ? Gender.Male : Gender.Female,
        isSACitizen: true,
        idNumber: `${String(birthYear).slice(2)}${pad(1 + Math.floor(rand() * 12), 2)}${pad(1 + Math.floor(rand() * 27), 2)}${pad(Math.floor(rand() * 10000), 4)}08${Math.floor(rand() * 10)}`,
        admissionNumber,
        admissionDate: `${YEAR}-01-15`,
        currentGradeId: cls.gradeId,
        currentClassId: cls.id,
        popiaConsentGiven: true,
      },
    });
    created++;
    studentsMade++;

    try {
      await call('/api/services/app/StudentParent/Link', {
        body: {
          studentId: student.id,
          parentId: parent.id,
          relationshipType: rand() > 0.5 ? Relationship.Mother : Relationship.Father,
          isPrimaryContact: true,
          isFinanciallyResponsible: true,
          canPickupStudent: true,
          livesWithStudent: true,
        },
      });
    } catch (e) {
      console.log(`\n    warn: could not link ${admissionNumber} to a parent — ${e.message}`);
    }
    process.stdout.write(`    ${studentsMade}/${STUDENT_COUNT}\r`);
  }
  console.log(`    ${studentsMade} students created        `);

  // 12. Fees -----------------------------------------------------------------
  step(12, 'Ensuring fee structures per grade');
  const existingFees = await listAll('FeeStructure');
  let fees = 0;
  for (const grade of grades) {
    for (const fee of [
      { feeType: FeeType.Tuition, feeName: `${grade.name} tuition ${YEAR}`, amount: grade.tuition, dueDay: 7 },
      { feeType: FeeType.Registration, feeName: `${grade.name} registration ${YEAR}`, amount: 2500, dueDay: 15 },
    ]) {
      if (existingFees.find((f) => f.feeName === fee.feeName)) { reused++; continue; }
      await call('/api/services/app/FeeStructure/Create', {
        body: { ...fee, gradeId: grade.id, academicYearId: year.id, currency: 'ZAR' },
      });
      created++;
      fees++;
    }
  }
  console.log(`    ${fees} fee structures created`);

  // Everything from here is colour rather than structure: if one module is
  // unavailable on this deployment the school is still usable, so these report
  // and carry on instead of aborting the run.
  const students = await listAll('Student');

  // 13. Timetables -----------------------------------------------------------
  step(13, 'Generating a timetable for every class');
  await soft('timetables', async () => {
    const existingTimetables = await listAll('Timetable');
    if (existingTimetables.length > 0) {
      console.log(`    ${existingTimetables.length} already exist — skipping generation`);
      reused++;
      return;
    }
    const result = await call('/api/services/app/Timetable/Generate', {
      body: {
        academicYearId: year.id,
        effectiveDate: `${YEAR}-01-15`,
        workingDays: [1, 2, 3, 4, 5],
        periodsPerDay: PERIODS_PER_DAY,
        periodStartTime: `${PERIOD_START}:00`,
        periodDurationMinutes: PERIOD_MINUTES,
        breakAfterPeriods: BREAK_AFTER_PERIODS,
        breakDurationMinutes: BREAK_MINUTES,
      },
    });
    created++;
    console.log(`    ${result.totalSlotsPlaced}/${result.totalSlotsRequested} periods placed across ${result.totalClasses} classes`);
    if (result.unplaced?.length) {
      console.log(`    ${result.unplaced.length} lessons could not be placed — visible on the generate page`);
    }
    // Generated timetables are drafts; activate so the class pages show them.
    const drafts = (result.classes ?? []).filter((c) => c.timetableId);
    await mapPool(drafts, 4, (c) =>
      call(`/api/services/app/Timetable/Activate?id=${c.timetableId}`).catch(() => null));
    console.log(`    ${drafts.length} activated`);
  });

  // 14. Attendance -----------------------------------------------------------
  step(14, `Capturing attendance for the last ${ATTENDANCE_DAYS} school days`);
  await soft('attendance', async () => {
    const days = [];
    for (let back = 1; days.length < ATTENDANCE_DAYS; back++) {
      const d = new Date();
      d.setDate(d.getDate() - back);
      if (d.getDay() !== 0 && d.getDay() !== 6) days.push(iso(d));
    }

    let registers = 0;
    for (const cls of classes) {
      const roll = students.filter((s) => s.currentClassId === cls.id);
      if (roll.length === 0) continue;
      const teacher = teachers[classes.indexOf(cls) % teachers.length];
      for (const day of days) {
        // A believable register: most present, a couple away, the odd late arrival.
        const entries = roll.map((s) => {
          const r = rand();
          const status = r > 0.94 ? 2 : r > 0.90 ? 3 : r > 0.88 ? 4 : 1;
          return {
            studentId: s.id,
            status,
            notes: status === 2 ? 'Parent phoned in' : status === 4 ? 'Medical appointment' : undefined,
          };
        });
        await call('/api/services/app/Attendance/BulkCapture', {
          body: { classId: cls.id, teacherId: teacher.id, attendanceDate: day, entries },
        });
        registers++;
        process.stdout.write(`    ${registers} registers\r`);
      }
    }
    created += registers;
    console.log(`    ${registers} class registers captured        `);
  });

  // 15. Announcements --------------------------------------------------------
  step(15, `Posting ${ANNOUNCEMENTS.length} announcements`);
  await soft('announcements', async () => {
    const existing = await listAll('Announcement');
    for (const a of ANNOUNCEMENTS) {
      if (existing.find((x) => x.title === a.title)) { reused++; continue; }
      await call('/api/services/app/Announcement/Create', {
        body: { ...a, publishDate: iso(new Date()), sendEmailNotification: false, sendPushNotification: false },
      });
      created++;
    }
    console.log(`    done`);
  });

  // 16. Fees and payments ----------------------------------------------------
  step(16, 'Billing tuition and recording payments');
  await soft('fees', async () => {
    const structures = await listAll('FeeStructure');
    const existingFees = await listAll('StudentFee');
    let billed = 0;

    for (const structure of structures) {
      const roll = students.filter((s) => s.currentGradeId === structure.gradeId);
      const unbilled = roll.filter(
        (s) => !existingFees.find((f) => f.studentId === s.id && f.feeStructureId === structure.id));
      if (unbilled.length === 0) { reused++; continue; }
      await call('/api/services/app/StudentFee/BulkCreate', {
        body: { feeStructureId: structure.id, studentIds: unbilled.map((s) => s.id) },
      });
      billed += unbilled.length;
      created += unbilled.length;
      process.stdout.write(`    ${billed} fees billed\r`);
    }
    console.log(`    ${billed} student fees billed        `);

    // Pay most of the tuition so the finance screens are not all arrears.
    const fees = await listAll('StudentFee');
    const payable = fees.filter((f) => (f.outstandingBalance ?? f.amountDue ?? 0) > 0);
    const toPay = payable.filter(() => rand() < PAID_FEE_RATIO);
    let paid = 0;
    await mapPool(toPay, 4, async (fee) => {
      const admissionIndex = Number(
        (students.find((s) => s.id === fee.studentId)?.admissionNumber ?? '').split('-')[1] ?? 0);
      const parent = parents[admissionIndex % parents.length];
      if (!parent) return;
      try {
        await call('/api/services/app/Payment/Create', {
          body: {
            studentId: fee.studentId,
            parentId: parent.id,
            amount: fee.outstandingBalance ?? fee.amountDue ?? 0,
            paymentMethod: pick([1, 2, 7, 10]),   // EFT, debit order, PayFast, Ozow
            paymentDate: iso(new Date()),
            paymentReference: `PMT-${pad(++paid, 5)}`,
          },
        });
        created++;
      } catch { /* a fee may already be settled on a re-run */ }
      process.stdout.write(`    ${paid} payments\r`);
    });
    console.log(`    ${paid} payments recorded        `);
  });

  // 17. The SA-specific programmes -------------------------------------------
  step(17, 'Extramurals, transport and aftercare, with learners enrolled');
  await soft('programmes', async () => {
    const enrolStart = `${YEAR}-01-20`;
    const sample = (n) => students.filter(() => rand() < n / students.length);

    // Extramurals
    const existingActivities = await listAll('ExtramuralActivity');
    for (const a of EXTRAMURALS) {
      let activity = existingActivities.find((x) => x.activityName === a.activityName);
      if (activity) { reused++; } else {
        activity = await call('/api/services/app/ExtramuralActivity/Create', {
          body: { ...a, academicYearId: year.id, maxCapacity: 30, description: `${a.activityName} at ${SCHOOL_NAME}` },
        });
        created++;
      }
      await mapPool(sample(18), 4, (s) =>
        call('/api/services/app/StudentExtramural/Create', {
          body: { studentId: s.id, extramuralActivityId: activity.id, academicYearId: year.id, startDate: enrolStart },
        }).then(() => created++).catch(() => null));
    }
    console.log(`    ${EXTRAMURALS.length} activities with learners enrolled`);

    // Transport
    const existingRoutes = await listAll('SchoolTransport');
    for (const r of TRANSPORT_ROUTES) {
      let route = existingRoutes.find((x) => x.routeName === r.routeName);
      if (route) { reused++; } else {
        route = await call('/api/services/app/SchoolTransport/Create', {
          body: { ...r, morningPickupTime: '06:45:00', afternoonDepartureTime: '14:30:00' },
        });
        created++;
      }
      await mapPool(sample(12), 4, (s) =>
        call('/api/services/app/StudentTransport/Create', {
          body: { studentId: s.id, schoolTransportId: route.id, academicYearId: year.id, direction: 3, startDate: enrolStart },
        }).then(() => created++).catch(() => null));
    }
    console.log(`    ${TRANSPORT_ROUTES.length} routes with learners enrolled`);

    // Aftercare
    const existingAfterCare = await listAll('AfterCare');
    const programmeName = 'Afternoon care';
    let programme = existingAfterCare.find((x) => x.programName === programmeName);
    if (programme) { reused++; } else {
      programme = await call('/api/services/app/AfterCare/Create', {
        body: {
          academicYearId: year.id, programName: programmeName, afterCareType: 1,
          location: 'Foundation Phase hall', startTime: '14:00:00', endTime: '17:30:00',
          daysAvailable: 'Mon–Fri', capacity: 60, supervisorName: 'Ms Arendse',
          contactPhone: '021 555 0111', includesMeals: true, includesHomeworkSupervision: true,
          monthlyFee: 1450, activitiesIncluded: 'Homework supervision, outdoor play, reading corner',
        },
      });
      created++;
    }
    await mapPool(sample(25), 4, (s) =>
      call('/api/services/app/StudentAfterCare/Create', {
        body: { studentId: s.id, afterCareId: programme.id, academicYearId: year.id, startDate: enrolStart, usualPickupTime: '17:00:00' },
      }).then(() => created++).catch(() => null));
    console.log(`    aftercare programme with learners enrolled`);
  });

  // 18. The approvals queue --------------------------------------------------
  step(18, 'Raising disciplinary cases, leave, trips, expenses and waivers');
  await soft('operations', async () => {
    const pickStudent = () => students[Math.floor(rand() * students.length)];

    const cases = await listAll('DisciplinaryCase');
    for (const c of DISCIPLINARY_CASES) {
      if (cases.find((x) => x.incidentDescription === c.incidentDescription)) { reused++; continue; }
      const d = new Date(); d.setDate(d.getDate() - Math.floor(rand() * 21));
      await call('/api/services/app/DisciplinaryCase/Create', {
        body: { ...c, studentId: pickStudent().id, academicYearId: year.id, incidentDate: iso(d) },
      });
      created++;
    }

    const leaves = await listAll('StaffLeaveRequest');
    for (const l of LEAVE_REQUESTS) {
      if (leaves.find((x) => x.reason === l.reason)) { reused++; continue; }
      const start = new Date(); start.setDate(start.getDate() + 7 + Math.floor(rand() * 21));
      const end = new Date(start); end.setDate(end.getDate() + l.days - 1);
      await call('/api/services/app/StaffLeaveRequest/Create', {
        body: { leaveType: l.leaveType, startDate: iso(start), endDate: iso(end), reason: l.reason },
      });
      created++;
    }

    const trips = await listAll('FieldTrip');
    for (const t of FIELD_TRIPS) {
      if (trips.find((x) => x.tripName === t.tripName)) { reused++; continue; }
      const d = new Date(); d.setDate(d.getDate() + 21 + Math.floor(rand() * 40));
      await call('/api/services/app/FieldTrip/Create', {
        body: {
          ...t, academicYearId: year.id, tripDate: iso(d),
          organizingTeacherId: teachers[Math.floor(rand() * teachers.length)].id,
        },
      });
      created++;
    }

    const expenses = await listAll('ExpenseRequest');
    for (const e of EXPENSES) {
      if (expenses.find((x) => x.description === e.description)) { reused++; continue; }
      const due = new Date(); due.setDate(due.getDate() + 14 + Math.floor(rand() * 30));
      await call('/api/services/app/ExpenseRequest/Create', {
        body: { ...e, academicYearId: year.id, requiredByDate: iso(due) },
      });
      created++;
    }

    const waivers = await listAll('FeeWaiver');
    for (const w of WAIVERS) {
      if (waivers.find((x) => x.reason === w.reason)) { reused++; continue; }
      const student = pickStudent();
      const grade = grades.find((g) => g.id === student.currentGradeId);
      await call('/api/services/app/FeeWaiver/Create', {
        body: {
          studentId: student.id, academicYearId: year.id, waiverType: w.waiverType,
          requestedAmount: Math.round((grade?.tuition ?? 24000) * w.share),
          reason: w.reason,
        },
      });
      created++;
    }
    console.log('    approvals queue populated');
  });

  // Done ---------------------------------------------------------------------
  console.log(`\nDone. Created ${created}, reused ${reused}.`);
  console.log(`\nSign in at the school with:`);
  console.log(`  tenancy   ${TENANCY}`);
  console.log(`  user      admin`);
  console.log(`  password  ${TENANT_PASSWORD === '123qwe' ? '123qwe (ABP default — change it)' : '(as configured)'}`);
  console.log(`\nTeachers are teacher001..teacher${pad(TEACHER_COUNT, 3)}, parents parent001..parent${pad(PARENT_COUNT, 3)}.`);
  console.log(`Note: approval workflows are not seeded — Tenant/SeedWorkflowDefinitions ships in PR #181.`);
}

main().catch((e) => {
  console.error(`\nFailed: ${e.message}`);
  console.error('The script is idempotent — fix the cause and re-run to resume.');
  process.exit(1);
});
