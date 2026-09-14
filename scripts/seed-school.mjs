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
