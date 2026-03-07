"use client";

import { Button, Card, Form, Input, Select, message } from "antd";
import {
  MailOutlined,
  PhoneOutlined,
  EnvironmentOutlined,
} from "@ant-design/icons";
import { useRouter } from "next/navigation";
import { useState } from "react";
import styles from "./contact.module.css";

const { TextArea } = Input;
const { Option } = Select;

export default function ContactPage() {
  const router = useRouter();
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (values: any) => {
    setLoading(true);
    // Simulate API call
    setTimeout(() => {
      message.success("Thank you! We will contact you shortly.");
      form.resetFields();
      setLoading(false);
    }, 1500);
  };

  return (
    <div className={styles.container}>
      {/* Header */}
      <header className={styles.header}>
        <div className={styles.headerContent}>
          <div className={styles.logo} onClick={() => router.push("/")}>
            PSMS
          </div>
          <nav className={styles.nav}>
            <a href="/#features">Features</a>
            <a href="/about">About</a>
            <a href="/contact">Contact</a>
            <Button type="primary" onClick={() => router.push("/auth/login")}>
              Login
            </Button>
          </nav>
        </div>
      </header>

      {/* Page Header */}
      <section className={styles.pageHeader}>
        <div className={styles.pageHeaderContent}>
          <h1>Contact Us</h1>
          <p>Get in touch with our team for demos, support, or inquiries</p>
        </div>
      </section>

      {/* Content */}
      <section className={styles.content}>
        <div className={styles.contentInner}>
          <div className={styles.grid}>
            {/* Contact Form */}
            <Card className={styles.formCard}>
              <h2>Send Us a Message</h2>
              <Form
                form={form}
                layout="vertical"
                onFinish={handleSubmit}
                size="large"
              >
                <Form.Item
                  label="Full Name"
                  name="name"
                  rules={[
                    { required: true, message: "Please enter your name" },
                  ]}
                >
                  <Input placeholder="John Doe" />
                </Form.Item>

                <Form.Item
                  label="Email Address"
                  name="email"
                  rules={[
                    { required: true, message: "Please enter your email" },
                    { type: "email", message: "Please enter a valid email" },
                  ]}
                >
                  <Input placeholder="john@school.co.za" />
                </Form.Item>

                <Form.Item
                  label="Phone Number"
                  name="phone"
                  rules={[
                    {
                      required: true,
                      message: "Please enter your phone number",
                    },
                  ]}
                >
                  <Input placeholder="+27 11 123 4567" />
                </Form.Item>

                <Form.Item
                  label="School Name"
                  name="school"
                  rules={[
                    {
                      required: true,
                      message: "Please enter your school name",
                    },
                  ]}
                >
                  <Input placeholder="ABC Private School" />
                </Form.Item>

                <Form.Item
                  label="Inquiry Type"
                  name="type"
                  rules={[
                    {
                      required: true,
                      message: "Please select an inquiry type",
                    },
                  ]}
                >
                  <Select placeholder="Select inquiry type">
                    <Option value="demo">Request Demo</Option>
                    <Option value="sales">Sales Inquiry</Option>
                    <Option value="support">Technical Support</Option>
                    <Option value="general">General Question</Option>
                  </Select>
                </Form.Item>

                <Form.Item
                  label="Message"
                  name="message"
                  rules={[
                    { required: true, message: "Please enter your message" },
                  ]}
                >
                  <TextArea
                    rows={4}
                    placeholder="Tell us about your requirements..."
                  />
                </Form.Item>

                <Form.Item>
                  <Button
                    type="primary"
                    htmlType="submit"
                    block
                    loading={loading}
                  >
                    Send Message
                  </Button>
                </Form.Item>
              </Form>
            </Card>

            {/* Contact Info */}
            <div className={styles.infoSection}>
              <Card className={styles.infoCard}>
                <MailOutlined className={styles.infoIcon} />
                <h3>Email</h3>
                <p>info@psms.co.za</p>
                <p>support@psms.co.za</p>
              </Card>

              <Card className={styles.infoCard}>
                <PhoneOutlined className={styles.infoIcon} />
                <h3>Phone</h3>
                <p>+27 (0) 11 123 4567</p>
                <p>Monday - Friday: 8:00 AM - 5:00 PM</p>
              </Card>

              <Card className={styles.infoCard}>
                <EnvironmentOutlined className={styles.infoIcon} />
                <h3>Office</h3>
                <p>123 Business Park</p>
                <p>Sandton, Johannesburg</p>
                <p>South Africa, 2196</p>
              </Card>

              <Card className={styles.demoCard}>
                <h3>Need Help Right Away?</h3>
                <p>
                  For existing customers, access our support portal or contact
                  your account manager directly.
                </p>
                <Button
                  block
                  size="large"
                  onClick={() => router.push("/auth/login")}
                >
                  Login to Support Portal
                </Button>
              </Card>
            </div>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className={styles.footer}>
        <div className={styles.footerContent}>
          <div className={styles.footerSection}>
            <h4>PSMS</h4>
            <p>Private School Management System</p>
            <p>Enterprise Edition</p>
          </div>
          <div className={styles.footerSection}>
            <h4>Product</h4>
            <a href="/#features">Features</a>
            <a href="/about">About</a>
            <a href="/contact">Contact</a>
          </div>
          <div className={styles.footerSection}>
            <h4>Resources</h4>
            <a href="#">Documentation</a>
            <a href="#">Support</a>
            <a href="#">Privacy Policy</a>
          </div>
          <div className={styles.footerSection}>
            <h4>Contact</h4>
            <p>Email: info@psms.co.za</p>
            <p>Phone: +27 (0) 11 123 4567</p>
          </div>
        </div>
        <div className={styles.footerBottom}>
          <p>© 2024 Private School Management System. All rights reserved.</p>
        </div>
      </footer>
    </div>
  );
}
