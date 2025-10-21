import React from 'react';
import '@fortawesome/fontawesome-free/css/all.min.css';

const Footer = () => {
  return (
    <footer style={{
      background: 'linear-gradient(135deg, #243483 0%, #1467EA 100%)',
      color: 'white',
      padding: '2rem 0',
      marginTop: 'auto'
    }}>
      <div style={{
        maxWidth: '1200px',
        margin: '0 auto',
        padding: '0 2rem'
      }}>
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))',
          gap: '2rem',
          marginBottom: '2rem'
        }}>
          {/* Company Info */}
          <div>
            <div style={{ 
              display: 'flex', 
              alignItems: 'center', 
              marginBottom: '1rem' 
            }}>
              <div style={{
                width: '32px',
                height: '32px',
                background: 'white',
                borderRadius: '6px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                marginRight: '0.5rem',
                color: '#243483',
                fontWeight: 'bold'
              }}>
                <i className="fas fa-university"></i>
              </div>
              <h3 style={{ margin: 0, fontSize: '1.2rem' }}>Bank Portal</h3>
            </div>
            <p style={{ 
              margin: 0, 
              opacity: 0.9, 
              lineHeight: 1.6,
              fontSize: '0.9rem'
            }}>
              Your trusted partner for banking registration and financial services. 
              Secure, reliable, and efficient banking solutions.
            </p>
          </div>

          {/* Quick Links */}
          <div>
            <h4 style={{ 
              margin: '0 0 1rem 0', 
              fontSize: '1rem',
              fontWeight: 'bold'
            }}>
              Quick Links
            </h4>
            <ul style={{ 
              listStyle: 'none', 
              padding: 0, 
              margin: 0 
            }}>
              {[
                { name: 'Home', path: '/' },
                { name: 'Register', path: '/register' },
                { name: 'Login', path: '/login' },
                { name: 'Dashboard', path: '/dashboard' }
              ].map(link => (
                <li key={link.name} style={{ marginBottom: '0.5rem' }}>
                  <a
                    href={link.path}
                    style={{
                      color: 'white',
                      textDecoration: 'none',
                      opacity: 0.9,
                      fontSize: '0.9rem',
                      transition: 'opacity 0.2s ease'
                    }}
                    onMouseEnter={(e) => e.target.style.opacity = '1'}
                    onMouseLeave={(e) => e.target.style.opacity = '0.9'}
                  >
                    {link.name}
                  </a>
                </li>
              ))}
            </ul>
          </div>

          {/* Contact Info */}
          <div>
            <h4 style={{ 
              margin: '0 0 1rem 0', 
              fontSize: '1rem',
              fontWeight: 'bold'
            }}>
              Contact Us
            </h4>
            <div style={{ fontSize: '0.9rem', opacity: 0.9 }}>
              <div style={{ 
                display: 'flex', 
                alignItems: 'center', 
                marginBottom: '0.5rem' 
              }}>
                <i className="fas fa-envelope" style={{ marginRight: '0.5rem' }}></i>
                <span>support@bankportal.com</span>
              </div>
              <div style={{ 
                display: 'flex', 
                alignItems: 'center', 
                marginBottom: '0.5rem' 
              }}>
                <i className="fas fa-phone" style={{ marginRight: '0.5rem' }}></i>
                <span>+1 (555) 123-4567</span>
              </div>
              <div style={{ 
                display: 'flex', 
                alignItems: 'center', 
                marginBottom: '0.5rem' 
              }}>
                <i className="fas fa-map-marker-alt" style={{ marginRight: '0.5rem' }}></i>
                <span>123 Banking Street, Financial District</span>
              </div>
            </div>
          </div>

          {/* Support */}
          <div>
            <h4 style={{ 
              margin: '0 0 1rem 0', 
              fontSize: '1rem',
              fontWeight: 'bold'
            }}>
              Support
            </h4>
            <ul style={{ 
              listStyle: 'none', 
              padding: 0, 
              margin: 0 
            }}>
              {[
                'Help Center',
                'Privacy Policy',
                'Terms of Service',
                'FAQ'
              ].map(item => (
                <li key={item} style={{ marginBottom: '0.5rem' }}>
                  <a
                    href="#"
                    style={{
                      color: 'white',
                      textDecoration: 'none',
                      opacity: 0.9,
                      fontSize: '0.9rem',
                      transition: 'opacity 0.2s ease'
                    }}
                    onMouseEnter={(e) => e.target.style.opacity = '1'}
                    onMouseLeave={(e) => e.target.style.opacity = '0.9'}
                    onClick={(e) => e.preventDefault()}
                  >
                    {item}
                  </a>
                </li>
              ))}
            </ul>
          </div>
        </div>

        {/* Bottom Bar */}
        <div style={{
          borderTop: '1px solid rgba(255,255,255,0.2)',
          paddingTop: '1.5rem',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: '1rem'
        }}>
          <div style={{ 
            fontSize: '0.9rem', 
            opacity: 0.8 
          }}>
            © 2024 Bank Portal. All rights reserved.
          </div>
          
          {/* Social Links */}
          <div style={{ 
            display: 'flex', 
            gap: '1rem' 
          }}>
            {[
              { icon: 'fab fa-facebook-f', name: 'Facebook' },
              { icon: 'fab fa-twitter', name: 'Twitter' },
              { icon: 'fab fa-linkedin-in', name: 'LinkedIn' },
              { icon: 'fab fa-instagram', name: 'Instagram' }
            ].map(social => (
              <a
                key={social.name}
                href="#"
                style={{
                  color: 'white',
                  textDecoration: 'none',
                  fontSize: '1.2rem',
                  opacity: 0.8,
                  transition: 'all 0.2s ease',
                  padding: '0.5rem',
                  borderRadius: '50%',
                  background: 'rgba(255,255,255,0.1)'
                }}
                onMouseEnter={(e) => {
                  e.target.style.opacity = '1';
                  e.target.style.transform = 'translateY(-2px)';
                  e.target.style.background = 'rgba(255,255,255,0.2)';
                }}
                onMouseLeave={(e) => {
                  e.target.style.opacity = '0.8';
                  e.target.style.transform = 'translateY(0)';
                  e.target.style.background = 'rgba(255,255,255,0.1)';
                }}
                onClick={(e) => e.preventDefault()}
                title={social.name}
              >
                <i className={social.icon}></i>
              </a>
            ))}
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
