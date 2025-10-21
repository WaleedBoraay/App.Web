import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import '@fortawesome/fontawesome-free/css/all.min.css';

const Welcome = () => {
  const navigate = useNavigate();
  const [scrollY, setScrollY] = useState(0);
  const [user, setUser] = useState(null);

  // Check if user is already logged in
  useEffect(() => {
    const userData = localStorage.getItem('user');
    if (userData) {
      try {
        const parsedUser = JSON.parse(userData);
        setUser(parsedUser);
        toast.success(`مرحباً بك ${parsedUser.firstName || parsedUser.username}! أنت مسجل دخول بالفعل.`, {
          position: "top-right",
          autoClose: 4000,
        });
      } catch (error) {
        console.error('Error parsing user data:', error);
      }
    }
  }, []);

  const handleLogout = () => {
    localStorage.removeItem('user');
    setUser(null);
    toast.success('تم تسجيل الخروج بنجاح', {
      position: "top-right",
      autoClose: 3000,
    });
  };

  useEffect(() => {
    const handleScroll = () => setScrollY(window.scrollY);
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  useEffect(() => {
    const observerOptions = {
      threshold: 0.2,
      rootMargin: '0px 0px -100px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add('animate');
        }
      });
    }, observerOptions);

    // Wait for component to mount and DOM to be ready
    const initializeObserver = () => {
      const animateElements = document.querySelectorAll('.animate-on-scroll');
      animateElements.forEach(el => {
        observer.observe(el);
      });
    };

    // Use both setTimeout and requestAnimationFrame for better timing
    const timer = setTimeout(() => {
      requestAnimationFrame(initializeObserver);
    }, 200);

    return () => {
      clearTimeout(timer);
      observer.disconnect();
    };
  }, []);

  return (
    <div className="home-page">
      {/* Header Navbar */}
      <header className="navbar">
        <div className="nav-container">
          <div className="nav-logo">
            <h2>
              <i className="fas fa-university" style={{ marginLeft: '0.5rem' }}></i>
              Bank Portal
            </h2>
          </div>
          <nav className="nav-links">
            <a href="#home" className="nav-link">Home</a>
            <a href="#services" className="nav-link">Services</a>
            <a href="#about" className="nav-link">About</a>
            <a href="#footer" className="nav-link">Contact</a>
          </nav>
          <div className="nav-buttons">
            {!user ? (
              <>
                <button 
                  className="btn btn-outline"
                  onClick={() => navigate('/login')}
                >
                  Login
                </button>
                <button 
                  className="btn btn-primary"
                  onClick={() => navigate('/register')}
                >
                  Register
                </button>
              </>
            ) : (
              <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
                {/* User Info */}
                <div 
                  onClick={() => navigate('/dashboard')}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '0.5rem',
                    cursor: 'pointer',
                    padding: '0.5rem 1rem',
                    borderRadius: '25px',
                    background: '#667eea',
                    transition: 'all 0.3s ease',
                    border: '1px solid rgba(255,255,255,0.2)'
                  }}
                  onMouseEnter={(e) => {
                    e.target.style.background = '#667eea';
                    e.target.style.transform = 'translateY(-1px)';
                  }}
                  onMouseLeave={(e) => {
                    e.target.style.background = '#667eea';
                    e.target.style.transform = 'translateY(0)';
                  }}
                >
                  <div style={{
                    width: '32px',
                    height: '32px',
                    background: 'white',
                    borderRadius: '50%',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    color: '#243483',
                    fontWeight: 'bold',
                    fontSize: '0.9rem'
                  }}>
                    {user.firstName ? user.firstName.charAt(0).toUpperCase() : user.username?.charAt(0).toUpperCase() || 'U'}
                  </div>
                  <span style={{ 
                    fontWeight: 'bold', 
                    color: 'white',
                    fontSize: '0.9rem'
                  }}>
                    {user.firstName ? `${user.firstName} ${user.lastName || ''}` : user.username}
                  </span>
                  <i className="fas fa-chevron-right" style={{ 
                    color: 'white', 
                    fontSize: '0.8rem',
                    opacity: 0.7
                  }}></i>
                </div>

                {/* Logout Button */}
                {/* <button 
                  className="btn btn-outline"
                  onClick={handleLogout}
                  style={{
                    padding: '0.5rem 1rem',
                    fontSize: '0.9rem'
                  }}
                >
                  <i className="fas fa-sign-out-alt" style={{ marginLeft: '0.5rem' }}></i>
                  Logout
                </button> */}
              </div>
            )}
          </div>
        </div>
      </header>

      {/* Hero Section */}
      <section id="home" className="hero-section">
        <div className="hero-background">
          <div className="floating-shapes">
            <div className="shape shape-1"></div>
            <div className="shape shape-2"></div>
            <div className="shape shape-3"></div>
          </div>
        </div>
        
        <div className="hero-container">
          <div className="hero-content" style={{ transform: `translateY(${scrollY * 0.5}px)` }}>
            <div className="hero-badge">
              <i className="fas fa-shield-alt"></i>
              <span>Trusted by 50,000+ customers</span>
            </div>
            <h1 className="hero-title">
              Your Financial Future
              <span className="gradient-text">Starts Here</span>
            </h1>
            <p className="hero-subtitle">
              Experience next-generation banking with advanced security, 
              seamless digital services, and personalized financial solutions.
            </p>
            <div className="hero-stats">
              <div className="stat-item">
                <div className="stat-number">99.9%</div>
                <div className="stat-label">Uptime</div>
              </div>
              <div className="stat-item">
                <div className="stat-number">24/7</div>
                <div className="stat-label">Support</div>
              </div>
              <div className="stat-item">
                <div className="stat-number">256-bit</div>
                <div className="stat-label">Encryption</div>
              </div>
            </div>
            <div className="hero-buttons">
              <button 
                className="btn btn-primary btn-large hero-cta"
                onClick={() => navigate('/register')}
              >
                <span>Get Started</span>
                <i className="fas fa-arrow-right"></i>
              </button>
              <button 
                className="btn btn-secondary btn-large"
                onClick={() => navigate('/login')}
              >
                <i className="fas fa-sign-in-alt"></i>
                <span>Sign In</span>
              </button>
            </div>
          </div>
          
          <div className="hero-visual">
            <div className="floating-card card-1" style={{ transform: `translateY(${scrollY * 0.3}px)` }}>
              <div className="card-header">
                <div className="card-chip"></div>
                <i className="fab fa-cc-visa"></i>
              </div>
              <div className="card-number">**** **** **** 1234</div>
              <div className="card-footer">
                <div className="card-holder">JOHN DOE</div>
                <div className="card-expiry">12/26</div>
              </div>
            </div>
            
            <div className="floating-card card-2" style={{ transform: `translateY(${scrollY * 0.2}px)` }}>
              <div className="transaction-item">
                <div className="transaction-icon success">
                  <i className="fas fa-check"></i>
                </div>
                <div className="transaction-details">
                  <div className="transaction-title">Payment Successful</div>
                  <div className="transaction-amount">+$2,500.00</div>
                </div>
              </div>
            </div>
            
            <div className="floating-card card-3" style={{ transform: `translateY(${scrollY * 0.4}px)` }}>
              <div className="security-badge">
                <i className="fas fa-lock"></i>
                <span>Bank-Level Security</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Services Section */}
      <section id="services" className="services-section">
        <div className="container">
          <h2 className="section-title">Our Banking Services</h2>
          <p className="section-subtitle">Comprehensive financial solutions tailored to meet your unique needs</p>
          
          <div className="services-grid">
            <div className="service-card animate-on-scroll">
              <div className="service-icon">
                <i className="fas fa-building"></i>
              </div>
              <h3>Corporate Banking</h3>
              <p>Complete business banking solutions including commercial loans, treasury services, and cash management for enterprises of all sizes.</p>
              <ul className="service-features">
                <li><i className="fas fa-check"></i> Business Loans & Credit Lines</li>
                <li><i className="fas fa-check"></i> Treasury Management</li>
                <li><i className="fas fa-check"></i> International Trade Finance</li>
                <li><i className="fas fa-check"></i> Payroll Services</li>
              </ul>
              <div className="service-cta">
                <button className="btn btn-outline-small">Learn More</button>
              </div>
            </div>

            <div className="service-card animate-on-scroll">
              <div className="service-icon">
                <i className="fas fa-piggy-bank"></i>
              </div>
              <h3>Personal Banking</h3>
              <p>Personalized banking services designed to help you achieve your financial goals with competitive rates and flexible terms.</p>
              <ul className="service-features">
                <li><i className="fas fa-check"></i> Savings & Checking Accounts</li>
                <li><i className="fas fa-check"></i> Personal Loans & Mortgages</li>
                <li><i className="fas fa-check"></i> Credit Cards & Debit Cards</li>
                <li><i className="fas fa-check"></i> Investment Advisory</li>
              </ul>
              <div className="service-cta">
                <button className="btn btn-outline-small">Learn More</button>
              </div>
            </div>

            <div className="service-card animate-on-scroll">
              <div className="service-icon">
                <i className="fas fa-mobile-alt"></i>
              </div>
              <h3>Digital Banking</h3>
              <p>State-of-the-art digital banking platform offering 24/7 access to your accounts with advanced security features.</p>
              <ul className="service-features">
                <li><i className="fas fa-check"></i> Mobile Banking App</li>
                <li><i className="fas fa-check"></i> Online Bill Pay</li>
                <li><i className="fas fa-check"></i> Digital Wallet Integration</li>
                <li><i className="fas fa-check"></i> Real-time Notifications</li>
              </ul>
              <div className="service-cta">
                <button className="btn btn-outline-small">Learn More</button>
              </div>
            </div>

            <div className="service-card animate-on-scroll">
              <div className="service-icon">
                <i className="fas fa-shield-alt"></i>
              </div>
              <h3>Security & Insurance</h3>
              <p>Advanced security measures and comprehensive insurance products to protect your assets and financial future.</p>
              <ul className="service-features">
                <li><i className="fas fa-check"></i> Fraud Protection</li>
                <li><i className="fas fa-check"></i> Life & Health Insurance</li>
                <li><i className="fas fa-check"></i> Identity Theft Protection</li>
                <li><i className="fas fa-check"></i> Safe Deposit Boxes</li>
              </ul>
              <div className="service-cta">
                <button className="btn btn-outline-small">Learn More</button>
              </div>
            </div>

            <div className="service-card animate-on-scroll">
              <div className="service-icon">
                <i className="fas fa-chart-line"></i>
              </div>
              <h3>Investment Services</h3>
              <p>Professional investment management and wealth planning services to help grow and preserve your wealth.</p>
              <ul className="service-features">
                <li><i className="fas fa-check"></i> Portfolio Management</li>
                <li><i className="fas fa-check"></i> Retirement Planning</li>
                <li><i className="fas fa-check"></i> Mutual Funds & ETFs</li>
                <li><i className="fas fa-check"></i> Financial Advisory</li>
              </ul>
              <div className="service-cta">
                <button className="btn btn-outline-small">Learn More</button>
              </div>
            </div>

            <div className="service-card animate-on-scroll">
              <div className="service-icon">
                <i className="fas fa-globe"></i>
              </div>
              <h3>International Banking</h3>
              <p>Global banking services for international transactions, foreign exchange, and cross-border business operations.</p>
              <ul className="service-features">
                <li><i className="fas fa-check"></i> Foreign Exchange</li>
                <li><i className="fas fa-check"></i> International Wire Transfers</li>
                <li><i className="fas fa-check"></i> Multi-Currency Accounts</li>
                <li><i className="fas fa-check"></i> Trade Finance Solutions</li>
              </ul>
              <div className="service-cta">
                <button className="btn btn-outline-small">Learn More</button>
              </div>
            </div>
          </div>

        
          
        </div>
      </section>

      {/* Login Section */}
      <section id="login" className="cta-section">
        <div className="container">
          <div className="cta-content">
            <h2>Ready to Get Started?</h2>
            <p>Join thousands of satisfied customers who trust us with their banking needs</p>
            <div className="cta-buttons">
              <button 
                className="btn btn-primary btn-large"
                onClick={() => navigate('/register')}
              >
                Open Account
              </button>
              <button 
                className="btn btn-outline btn-large"
                onClick={() => navigate('/login')}
              >
                Access Account
              </button>
            </div>
          </div>
        </div>
      </section>

      {/* About Section */}
      <section id="about" className="about-section">
        <div className="container">
          <div className="about-grid">
            <div className="about-content animate-on-scroll">
              <h2>About Bank Portal</h2>
              <p>We are committed to providing secure, reliable, and innovative banking solutions. With over 20 years of experience, we've built a reputation for excellence in financial services.</p>
              <div className="stats">
                <div className="stat">
                  <h3>1M+</h3>
                  <p>Happy Customers</p>
                </div>
                <div className="stat">
                  <h3>50+</h3>
                  <p>Countries</p>
                </div>
                <div className="stat">
                  <h3>24/7</h3>
                  <p>Support</p>
                </div>
              </div>
            </div>
            <div className="about-image animate-on-scroll">
              <div className="image-placeholder">
                <div className="placeholder-icon"><i className="fas fa-university"></i></div>
                <p>Trusted Banking Since 2003</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Footer */}
     
    </div>
  );
};

export default Welcome;
