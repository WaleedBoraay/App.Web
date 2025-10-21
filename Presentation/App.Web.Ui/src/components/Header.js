import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { toast } from 'react-toastify';
import '@fortawesome/fontawesome-free/css/all.min.css';

const Header = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [user, setUser] = useState(null);
  const [showNotifications, setShowNotifications] = useState(false);
  const [notifications] = useState([
    { id: 1, message: 'Welcome to Bank Portal', time: '5 min ago', read: false },
    { id: 2, message: 'Registration completed successfully', time: '1 hour ago', read: true },
    { id: 3, message: 'New update available', time: '2 hours ago', read: false }
  ]);

  useEffect(() => {
    // Check if user is logged in
    const userData = localStorage.getItem('user');
    if (userData) {
      try {
        setUser(JSON.parse(userData));
      } catch (error) {
        console.error('Error parsing user data:', error);
      }
    }
  }, [location]);

  const handleLogout = () => {
    localStorage.removeItem('user');
    localStorage.removeItem('authToken');
    localStorage.removeItem('registrationStep');
    localStorage.removeItem('step1Completed');
    setUser(null);
    toast.success('تم تسجيل الخروج بنجاح', {
      position: "top-right",
      autoClose: 3000,
    });
    navigate('/');
  };

  const unreadCount = notifications.filter(n => !n.read).length;

  return (
    <header style={{
      background: 'linear-gradient(135deg, #fff 0%, #eee 100%)',
      color: '#243483',
      padding: '1rem 2rem',
      boxShadow: '0 2px 10px rgba(0,0,0,0.1)',
      position: 'sticky',
      top: 0,
      zIndex: 1000
    }}>
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        maxWidth: '1200px',
        flexWrap:'wrap',
        margin: '0 auto'
      }}>
        {/* Logo and Title */}
        <div 
          style={{ 
            display: 'flex', 
            alignItems: 'center', 
            cursor: 'pointer' 
          }}
          onClick={() => navigate('/')}
        >
          <div style={{
            width: '40px',
            height: '40px',
            background: 'white',
            borderRadius: '8px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            flexWrap:'wrap',
            marginRight: '1rem',
            color: '#243483',
            fontWeight: 'bold',
            fontSize: '1.2rem'
          }}>
            <i className="fas fa-university"></i>
          </div>
          <h1 style={{ 
            margin: 0, 
            fontSize: '1.5rem',
            fontWeight: 'bold'
          }}>
            Bank Portal
          </h1>
        </div>

        {/* Navigation Links */}
        <nav style={{ display: 'flex', gap: '2rem', alignItems: 'center' }}>
          {!user ? (
            <>
              <button
                onClick={() => navigate('/login')}
                style={{
                  background: 'transparent',
                  border: '2px solid rgb(36, 52, 131)',
                  color: 'rgb(36, 52, 131)',
                  padding: '0.5rem 1rem',
                  borderRadius: '6px',
                  cursor: 'pointer',
                  fontWeight: 'bold',
                  transition: 'all 0.2s ease'
                }}
                onMouseEnter={(e) => {
                  e.target.style.background = 'white';
                  e.target.style.color = '#243483';
                }}
                onMouseLeave={(e) => {
                  e.target.style.background = 'transparent';
                  e.target.style.color = 'rgb(36, 52, 131)';
                }}
              >
                Login
              </button>
              <button
                onClick={() => navigate('/register')}
                style={{
                  background: '#243483 ',
                  border: 'none',
                  color: '#fff',
                  padding: '0.5rem 1rem',
                  borderRadius: '6px',
                  cursor: 'pointer',
                  fontWeight: 'bold',
                  transition: 'all 0.2s ease'
                }}
                onMouseEnter={(e) => {
                  e.target.style.transform = 'translateY(-1px)';
                  e.target.style.boxShadow = '0 2px 8px rgba(255,255,255,0.3)';
                }}
                onMouseLeave={(e) => {
                  e.target.style.transform = 'translateY(0)';
                  e.target.style.boxShadow = 'none';
                }}
              >
                Register
              </button>
            </>
          ) : (
            <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' ,flexWrap:'wrap'}}>
              {/* Notifications */}
              <div style={{ position: 'relative' }}>
                <button
                  onClick={() => setShowNotifications(!showNotifications)}
                  style={{
                    background: 'transparent',
                    border: 'none',
                    color: 'white',
                    cursor: 'pointer',
                    fontSize: '1.2rem',
                    position: 'relative',
                    padding: '0.5rem'
                  }}
                >
                  <i className="fas fa-bell" style={{color:"#243483"}}></i>
                  {unreadCount > 0 && (
                    <span style={{
                      position: 'absolute',
                      top: '0',
                      right: '0',
                      background: '#ff4757',
                      color: 'white',
                      borderRadius: '50%',
                      width: '18px',
                      height: '18px',
                      fontSize: '0.7rem',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center'
                    }}>
                      {unreadCount}
                    </span>
                  )}
                </button>

                {/* Notifications Dropdown */}
                {showNotifications && (
                  <div style={{
                    position: 'absolute',
                    top: '100%',
                    right: '0',
                    background: 'white',
                    color: '#333',
                    borderRadius: '8px',
                    boxShadow: '0 4px 20px rgba(0,0,0,0.15)',
                    width: '300px',
                    maxHeight: '400px',
                    overflowY: 'auto',
                    zIndex: 1001,
                    marginTop: '0.5rem'
                  }}>
                    <div style={{
                      padding: '1rem',
                      borderBottom: '1px solid #eee',
                      fontWeight: 'bold',
                      color: '#243483'
                    }}>
                      Notifications
                    </div>
                    {notifications.map(notification => (
                      <div
                        key={notification.id}
                        style={{
                          padding: '1rem',
                          borderBottom: '1px solid #eee',
                          background: notification.read ? 'white' : '#f8f9ff',
                          cursor: 'pointer'
                        }}
                        onClick={() => setShowNotifications(false)}
                      >
                        <div style={{ fontSize: '0.9rem', marginBottom: '0.5rem' }}>
                          {notification.message}
                        </div>
                        <div style={{ fontSize: '0.7rem', color: '#666' }}>
                          {notification.time}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>

            
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <div style={{
                  width: '32px',
                  height: '32px',
                  background: 'white',
                  borderRadius: '50%',
                  display: 'flex',
                  flexWrap:"wrap",
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: '#243483',
                  fontWeight: 'bold'
                }}>
                  {user.firstName ? user.firstName.charAt(0).toUpperCase() : user.username?.charAt(0).toUpperCase() || 'U'}
                </div>
                {/* <span style={{ fontWeight: 'bold' }}>
                  {user.firstName ? `${user.firstName} ${user.lastName || ''}` : user.username}
                </span> */}
              </div>

              {/* Logout Button */}
              <button
                onClick={handleLogout}
                style={{
                  background: 'transparent',
                  border: '2px solid #243483 ',
                  color: '#243483',
                  padding: '0.5rem 1rem',
                  borderRadius: '6px',
                  cursor: 'pointer',
                  fontWeight: 'bold',
                  transition: 'all 0.2s ease'
                }}
                onMouseEnter={(e) => {
                  e.target.style.background = 'white';
                  e.target.style.color = '#243483';
                }}
                onMouseLeave={(e) => {
                  e.target.style.background = 'transparent';
                  e.target.style.color = '#243483';
                }}
              >
                Logout
              </button>
            </div>
          )}
        </nav>
      </div>

      {/* Close notifications when clicking outside */}
      {showNotifications && (
        <div
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            zIndex: 999
          }}
          onClick={() => setShowNotifications(false)}
        />
      )}
    </header>
  );
};

export default Header;
