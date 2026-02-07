"use client";

import { Button, Card, Row, Col } from "antd";
import {
  BookOutlined,
  TeamOutlined,
  DollarOutlined,
  FileTextOutlined,
  SafetyOutlined,
  CloudOutlined,
} from "@ant-design/icons";
import { useRouter } from "next/navigation";
import styles from "./landing.module.css";

export default function LandingPage() {
  const router = useRouter();

  return (
    <div className={styles.container}>
      {/* Header */}
      <header className={styles.header}>
        <div className={styles.headerContent}>
          <div className={styles.logo}>PSMS</div>
          <nav className={styles.nav}>
            <a href="#features">Features</a>
            <a href="/about">About</a>
            <a href="/contact">Contact</a>
            <Button type="primary" onClick={() => router.push("/auth/login")}>
              Login
            </Button>
          </nav>
        </div>
      </header>

      {/* Hero Section */}
      <section className={styles.hero}>
        <div className={styles.heroContent}>
          <h1 className={styles.heroTitle}>
            Private School Management System
          </h1>
          <p className={styles.heroSubtitle}>
            Enterprise-grade school management solution for South African private
            schools. Streamline admissions, academics, finance, and
            communications.
          </p>
          <div className={styles.heroActions}>
            <Button
              type="primary"
              size="large"
              onClick={() => router.push("/auth/login")}
            >
              Get Started
            </Button>
            <Button size="large" onClick={() => router.push("/contact")}>
              Request Demo
            </Button>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section id="features" className={styles.features}>
        <div className={styles.sectionContent}>
          <h2 className={styles.sectionTitle}>Comprehensive Features</h2>
          <p className={styles.sectionSubtitle}>
            Everything you need to manage your private school efficiently
          </p>

          <Row gutter={[24, 24]} style={{ marginTop: "48px" }}>
            <Col xs={24} md={12} lg={8}>
              <Card className={styles.featureCard}>
                <TeamOutlined className={styles.featureIcon} />
                <h3>Academic Management</h3>
                <p>
                  Manage students, teachers, classes, subjects, assessments, and
                  reports. Complete academic lifecycle management.
                </p>
              </Card>
            </Col>
            <Col xs={24} md={12} lg={8}>
              <Card className={styles.featureCard}>
                <FileTextOutlined className={styles.featureIcon} />
                <h3>Admissions Portal</h3>
                <p>
                  Streamline application processing, interviews, assessments, and
                  enrollment with automated workflows.
                </p>
              </Card>
            </Col>
            <Col xs={24} md={12} lg={8}>
              <Card className={styles.featureCard}>
                <DollarOutlined className={styles.featureIcon} />
                <h3>Financial Management</h3>
                <p>
                  Fee structures, invoicing, payment tracking, and comprehensive
                  financial reporting in South African Rand.
                </p>
              </Card>
            </Col>
            <Col xs={24} md={12} lg={8}>
              <Card className={styles.featureCard}>
                <BookOutlined className={styles.featureIcon} />
                <h3>Learning Resources</h3>
                <p>
                  Upload and share learning materials, schedule online lessons,
                  and manage virtual classrooms.
                </p>
              </Card>
            </Col>
            <Col xs={24} md={12} lg={8}>
              <Card className={styles.featureCard}>
                <SafetyOutlined className={styles.featureIcon} />
                <h3>Multi-Tenant Security</h3>
                <p>
                  Enterprise-grade security with complete data isolation between
                  schools. Role-based access control.
                </p>
              </Card>
            </Col>
            <Col xs={24} md={12} lg={8}>
              <Card className={styles.featureCard}>
                <CloudOutlined className={styles.featureIcon} />
                <h3>Cloud-Based</h3>
                <p>
                  Access from anywhere, automatic backups, and 99.9% uptime
                  guarantee. No infrastructure management required.
                </p>
              </Card>
            </Col>
          </Row>
        </div>
      </section>

      {/* SA Specific Features */}
      <section className={styles.saFeatures}>
        <div className={styles.sectionContent}>
          <h2 className={styles.sectionTitle}>Built for South African Schools</h2>
          <Row gutter={[32, 32]} align="middle" style={{ marginTop: "48px" }}>
            <Col xs={24} lg={12}>
              <h3 style={{ fontSize: "20px", marginBottom: "16px" }}>
                Compliant with SA Requirements
              </h3>
              <ul className={styles.featureList}>
                <li>SA academic year calendar (January to December)</li>
                <li>4-term system with proper term management</li>
                <li>Grade levels: Foundation, Intermediate, Senior, FET</li>
                <li>After-care and extramural activity management</li>
                <li>School transport coordination</li>
                <li>Currency in South African Rand (R)</li>
              </ul>
            </Col>
            <Col xs={24} lg={12}>
              <Card className={styles.demoCard}>
                <h3 style={{ marginTop: 0 }}>Ready to see it in action?</h3>
                <p>
                  Schedule a personalized demo to see how PSMS can transform your
                  school management.
                </p>
                <Button
                  type="primary"
                  size="large"
                  block
                  onClick={() => router.push("/contact")}
                >
                  Request Demo
                </Button>
              </Card>
            </Col>
          </Row>
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
            <a href="#features">Features</a>
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
