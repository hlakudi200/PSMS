"use client";

import { Button, Card, Row, Col } from "antd";
import {
  RocketOutlined,
  TeamOutlined,
  SafetyOutlined,
  GlobalOutlined,
} from "@ant-design/icons";
import { useRouter } from "next/navigation";
import styles from "./about.module.css";

export default function AboutPage() {
  const router = useRouter();

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
          <h1>About PSMS</h1>
          <p>
            Empowering South African private schools with enterprise-grade
            management solutions
          </p>
        </div>
      </section>

      {/* Content */}
      <section className={styles.content}>
        <div className={styles.contentInner}>
          <Row gutter={[48, 48]}>
            <Col xs={24} lg={12}>
              <h2>Our Mission</h2>
              <p className={styles.paragraph}>
                PSMS was built to address the unique challenges faced by South
                African private schools. We understand that managing a school
                involves complex workflows across admissions, academics,
                finance, and communications.
              </p>
              <p className={styles.paragraph}>
                Our mission is to provide a comprehensive, enterprise-grade
                solution that streamlines operations, improves efficiency, and
                enhances the educational experience for students, teachers, and
                parents.
              </p>
            </Col>
            <Col xs={24} lg={12}>
              <h2>Why Choose PSMS?</h2>
              <p className={styles.paragraph}>
                Unlike generic school management systems, PSMS is specifically
                designed for the South African educational context. We support
                the SA academic calendar, CAPS curriculum alignment, and local
                compliance requirements.
              </p>
              <p className={styles.paragraph}>
                Built with modern technology and enterprise best practices, PSMS
                scales from small independent schools to large multi-campus
                institutions.
              </p>
            </Col>
          </Row>

          <div className={styles.divider}></div>

          <h2 className={styles.centered}>Core Values</h2>
          <Row gutter={[24, 24]} style={{ marginTop: "48px" }}>
            <Col xs={24} md={12} lg={6}>
              <Card className={styles.valueCard}>
                <RocketOutlined className={styles.valueIcon} />
                <h3>Innovation</h3>
                <p>
                  Leveraging cutting-edge technology to solve real educational
                  challenges.
                </p>
              </Card>
            </Col>
            <Col xs={24} md={12} lg={6}>
              <Card className={styles.valueCard}>
                <SafetyOutlined className={styles.valueIcon} />
                <h3>Security</h3>
                <p>
                  Enterprise-grade security with multi-tenant data isolation and
                  encryption.
                </p>
              </Card>
            </Col>
            <Col xs={24} md={12} lg={6}>
              <Card className={styles.valueCard}>
                <TeamOutlined className={styles.valueIcon} />
                <h3>Partnership</h3>
                <p>
                  Working closely with schools to understand and meet their
                  unique needs.
                </p>
              </Card>
            </Col>
            <Col xs={24} md={12} lg={6}>
              <Card className={styles.valueCard}>
                <GlobalOutlined className={styles.valueIcon} />
                <h3>Excellence</h3>
                <p>
                  Committed to delivering the highest quality software and
                  support.
                </p>
              </Card>
            </Col>
          </Row>

          <div className={styles.divider}></div>

          <div className={styles.ctaSection}>
            <h2>Ready to Transform Your School?</h2>
            <p>
              Join leading South African private schools using PSMS to streamline
              their operations.
            </p>
            <div className={styles.ctaButtons}>
              <Button
                type="primary"
                size="large"
                onClick={() => router.push("/contact")}
              >
                Request Demo
              </Button>
              <Button size="large" onClick={() => router.push("/auth/login")}>
                Login
              </Button>
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
