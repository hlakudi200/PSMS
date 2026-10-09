'use client';

import React, { useMemo } from 'react';
import { Alert, Col, DatePicker, Form, Input, Radio, Row, Select, Typography } from 'antd';
import dayjs from 'dayjs';
import type { IOpenIntake } from '@/providers/admissions/admission_settings/context';
import { ageAgainstIntake, documentsAsList } from './schema';

const { Text } = Typography;

/**
 * Step one: who the learner is, and what they are applying for.
 *
 * The intake comes first because everything after it depends on the answer —
 * the fee, the documents, the age the school expects. A parent choosing a
 * grade should see the terms attached to it before filling anything in.
 */
export default function LearnerStep({
  form,
  intakes,
  locked,
}: {
  form: ReturnType<typeof Form.useForm>[0];
  intakes: IOpenIntake[];
  locked: boolean;
}) {
  const chosen = Form.useWatch('intake', form) as string | undefined;
  const isSACitizen = Form.useWatch('isSACitizen', form) as boolean | undefined;
  const dateOfBirth = Form.useWatch('dateOfBirth', form);

  const intake = useMemo(
    () => intakes.find((i) => `${i.academicYearId}|${i.gradeId ?? ''}` === chosen),
    [intakes, chosen]
  );

  const ageNote = ageAgainstIntake(dateOfBirth, intake?.minimumAge, intake?.maximumAge);
  const documents = documentsAsList(intake?.requiredDocuments);

  return (
    <Form form={form} layout="vertical" disabled={locked}>
      <Form.Item
        label="What are you applying for?"
        name="intake"
        rules={[{ required: true, message: 'Choose what you are applying for.' }]}
      >
        <Select
          placeholder={intakes.length ? 'Choose a year and grade' : 'Nothing is open at the moment'}
          disabled={!intakes.length || locked}
          options={intakes.map((i) => ({
            value: `${i.academicYearId}|${i.gradeId ?? ''}`,
            label: `${i.gradeName ?? 'Any grade'} · ${i.academicYearName}`,
          }))}
        />
      </Form.Item>

      {intake && (
        <Alert
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          message={`${intake.gradeName ?? 'Any grade'} · ${intake.academicYearName}`}
          description={
            <>
              <div>
                {intake.feeRequired
                  ? `Application fee: R ${intake.feeAmount.toFixed(2)}, payable once you submit.`
                  : 'There is no application fee for this grade.'}
              </div>
              {intake.applicationCloseDate && (
                <div>Applications close on {dayjs(intake.applicationCloseDate).format('DD MMM YYYY')}.</div>
              )}
              {intake.availableSpots != null && <div>{intake.availableSpots} places left.</div>}
              {(intake.isInterviewRequired || intake.isAssessmentRequired) && (
                <div>
                  The school requires{' '}
                  {[intake.isInterviewRequired && 'an interview', intake.isAssessmentRequired && 'an assessment']
                    .filter(Boolean)
                    .join(' and ')}
                  .
                </div>
              )}
              {documents.length > 0 && (
                <div style={{ marginTop: 6 }}>
                  <Text strong>You will be asked for:</Text>
                  <ul style={{ margin: '4px 0 0 18px' }}>
                    {documents.map((d) => (
                      <li key={d}>{d}</li>
                    ))}
                  </ul>
                </div>
              )}
            </>
          }
        />
      )}

      <Row gutter={16}>
        <Col xs={24} md={8}>
          <Form.Item label="First name" name="firstName" rules={[{ required: true, message: "Enter the learner's first name." }]}>
            <Input placeholder="First name" />
          </Form.Item>
        </Col>
        <Col xs={24} md={8}>
          <Form.Item label="Middle name" name="middleName">
            <Input placeholder="Optional" />
          </Form.Item>
        </Col>
        <Col xs={24} md={8}>
          <Form.Item label="Surname" name="lastName" rules={[{ required: true, message: "Enter the learner's surname." }]}>
            <Input placeholder="Surname" />
          </Form.Item>
        </Col>
      </Row>

      <Row gutter={16}>
        <Col xs={24} md={12}>
          <Form.Item
            label="Date of birth"
            name="dateOfBirth"
            rules={[{ required: true, message: 'Enter the date of birth.' }]}
            validateStatus={ageNote ? 'warning' : undefined}
            help={ageNote}
          >
            <DatePicker
              style={{ width: '100%' }}
              format="DD MMM YYYY"
              /* A date of birth is always in the past, and typing a year is
                 faster than paging back fifteen of them. */
              picker="date"
              disabledDate={(d) => d && d.isAfter(dayjs(), 'day')}
            />
          </Form.Item>
        </Col>
        <Col xs={24} md={12}>
          <Form.Item label="Gender" name="gender" rules={[{ required: true, message: 'Choose one.' }]}>
            <Select
              placeholder="As it appears on the birth certificate"
              options={[
                { value: 1, label: 'Male' },
                { value: 2, label: 'Female' },
                { value: 3, label: 'Other' },
              ]}
            />
          </Form.Item>
        </Col>
      </Row>

      <Form.Item label="Is the learner a South African citizen?" name="isSACitizen">
        <Radio.Group>
          <Radio value>Yes</Radio>
          <Radio value={false}>No</Radio>
        </Radio.Group>
      </Form.Item>

      {isSACitizen === false ? (
        <Form.Item
          label="Passport number"
          name="passportNumber"
          rules={[{ required: true, message: 'A passport number is required.' }]}
        >
          <Input placeholder="Passport number" />
        </Form.Item>
      ) : (
        <Form.Item
          label="ID number"
          name="idNumber"
          extra="Thirteen digits, as on the birth certificate or ID."
          rules={[{ required: true, message: 'An ID number is required.' }]}
        >
          <Input placeholder="0000000000000" maxLength={13} inputMode="numeric" />
        </Form.Item>
      )}

      <Form.Item label="Previous school" name="previousSchool" extra="Leave empty if this is the learner's first school.">
        <Input placeholder="Name of the school they are coming from" />
      </Form.Item>
    </Form>
  );
}
