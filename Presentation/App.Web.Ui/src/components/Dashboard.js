import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import AdminDashboard from './admin/AdminDashboard';
import '@fortawesome/fontawesome-free/css/all.min.css';



const Dashboard = () => {
  const navigate = useNavigate();
  const [user, setUser] = useState(null);
  const [isSuperAdmin, setIsSuperAdmin] = useState(false);


  useEffect(() => {
    const userData = localStorage.getItem('user');
    if (!userData) {
      navigate('/');
      return;
    }
    
    try {
      const parsedUser = JSON.parse(userData);
      console.log('Dashboard - User data:', parsedUser);
      
      // Check if user has Super Admin role
      const hasSuperAdminRole = Array.isArray(parsedUser.roles) && 
                               parsedUser.roles.includes('Super Admin');
      
      setIsSuperAdmin(hasSuperAdminRole);
      setUser(parsedUser);
    } catch (error) {
      console.error('Error parsing user data:', error);
      navigate('/');
    }
  }, [navigate]);

  const handleLogout = () => {
    localStorage.removeItem('user');
    navigate('/');
  };

  if (!user) {
    return (
      <div style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        color: 'white'
      }}>
        <div style={{ textAlign: 'center' }}>
          <i className="fas fa-spinner fa-spin" style={{ fontSize: '2rem', marginBottom: '1rem' }}></i>
          <p>Loading Dashboard...</p>
        </div>
      </div>
    );
  }


  // Render Super Admin Dashboard
  if (isSuperAdmin) {
    return <AdminDashboard user={user} />;
  }

  // Regular User Dashboard
  return (
    <div style={{
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)',
      padding: '2rem 0'
    }}>
      <div style={{
        maxWidth: '1200px',
        margin: '0 auto',
        padding: '0 2rem'
      }}>
        {/* Welcome Header */}
        <div style={{
          background: 'linear-gradient(135deg, #243483 0%, #1467EA 100%)',
          color: 'white',
          padding: '2rem',
          borderRadius: '15px',
          marginBottom: '2rem',
          boxShadow: '0 10px 30px rgba(0,0,0,0.1)'
        }}>
          <div style={{ display: 'flex', alignItems: 'center', marginBottom: '1rem' }}>
            <div style={{
              width: '60px',
              height: '60px',
              background: 'rgba(255,255,255,0.2)',
              borderRadius: '50%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              marginRight: '1rem'
            }}>
              <i className="fas fa-user" style={{ fontSize: '1.5rem' }}></i>
            </div>
            <div>
              <h1 style={{ margin: 0, fontSize: '2rem' }}>Welcome, {user.username}!</h1>
              <p style={{ margin: '0.5rem 0 0 0', opacity: 0.9 }}>
                {user.isActive ? 'You can complete your registration' : 'Your account is fully activated'}
              </p>
            </div>
          </div>
        </div>

        {/* Dashboard Grid */}
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(350px, 1fr))',
          gap: '2rem',
          marginBottom: '2rem'
        }}>
          
          {/* User Information Card */}
          <div style={{
            background: 'white',
            padding: '2rem',
            borderRadius: '15px',
            boxShadow: '0 8px 25px rgba(0,0,0,0.1)',
            border: '1px solid #e9ecef'
          }}>
            <div style={{ display: 'flex', alignItems: 'center', marginBottom: '1.5rem' }}>
              <i className="fas fa-user-circle" style={{ 
                fontSize: '1.5rem', 
                color: '#243483', 
                marginRight: '0.5rem' 
              }}></i>
              <h3 style={{ margin: 0, color: '#243483' }}>User Information</h3>
            </div>
            
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ color: '#6c757d', fontWeight: '500' }}>Username:</span>
                <span style={{ fontWeight: 'bold', color: '#495057' }}>{user.username}</span>
              </div>
              
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ color: '#6c757d', fontWeight: '500' }}>Email:</span>
                <span style={{ fontWeight: 'bold', color: '#495057' }}>{user.email}</span>
              </div>
              
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ color: '#6c757d', fontWeight: '500' }}>Account Status:</span>
                <span style={{ 
                  fontWeight: 'bold', 
                  color: user.isActive ? '#fff' : '#fff',
                  background: user.isActive ? '#28a745' : '#28a745',
                  padding: '0.25rem 0.75rem',
                  borderRadius: '20px',
                  fontSize: '0.85rem'
                }}>
                  {user.isActive ? 'Pending Completion' : 'inActive'}
                </span>
              </div>
              
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ color: '#6c757d', fontWeight: '500' }}>Created Date:</span>
                <span style={{ fontWeight: 'bold', color: '#495057' }}>
                  {user.institution?.createdOnUtc ? 
                    new Date(user.institution.createdOnUtc).toLocaleDateString('en-US') : 
                    'Not specified'
                  }
                </span>
              </div>

              {/* Continue to Step 2 Button - Only show if user is active */}
              {user.isActive && (
                <div style={{ 
                  marginTop: '1rem', 
                  padding: '1rem',
                  background: 'linear-gradient(135deg, #28a745 0%, #20c997 100%)',
                  borderRadius: '10px',
                  textAlign: 'center'
                }}>
                  <div style={{ marginBottom: '0.5rem' }}>
                    <i className="fas fa-info-circle" style={{ color: 'white', fontSize: '1.2rem' }}></i>
                  </div>
                  <p style={{ color: 'white', margin: '0 0 1rem 0', fontSize: '0.9rem' }}>
                    Complete your registration by continuing to Step 2
                  </p>
                  <button
                    onClick={() => {
                      // Set registration step to 1 (0-indexed, so step 2)
                      localStorage.setItem('registrationStep', '1');
                      localStorage.setItem('step1Completed', 'true');
                      localStorage.setItem('fromDashboard', 'true');
                      navigate('/register');
                    }}
                    style={{
                      background: 'white',
                      color: '#28a745',
                      border: 'none',
                      padding: '0.75rem 1.5rem',
                      borderRadius: '25px',
                      fontWeight: 'bold',
                      cursor: 'pointer',
                      transition: 'all 0.3s ease',
                      fontSize: '0.9rem'
                    }}
                    onMouseEnter={(e) => {
                      e.target.style.transform = 'translateY(-2px)';
                      e.target.style.boxShadow = '0 5px 15px rgba(255,255,255,0.3)';
                    }}
                    onMouseLeave={(e) => {
                      e.target.style.transform = 'translateY(0)';
                      e.target.style.boxShadow = 'none';
                    }}
                  >
                    <i className="fas fa-arrow-right" style={{ marginRight: '0.5rem' }}></i>
                    Continue to Step 2
                  </button>
                </div>
              )}
            </div>
          </div>

          {/* Institution Information Card */}
          {user.institution && (
            <div style={{
              background: 'white',
              padding: '2rem',
              borderRadius: '15px',
              boxShadow: '0 8px 25px rgba(0,0,0,0.1)',
              border: '1px solid #e9ecef'
            }}>
              <div style={{ display: 'flex', alignItems: 'center', marginBottom: '1.5rem' }}>
                <i className="fas fa-university" style={{ 
                  fontSize: '1.5rem', 
                  color: '#243483', 
                  marginRight: '0.5rem' 
                }}></i>
                <h3 style={{ margin: 0, color: '#243483' }}>Institution Information</h3>
              </div>
              
              <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ color: '#6c757d', fontWeight: '500' }}>Institution Name:</span>
                  <span style={{ fontWeight: 'bold', color: '#495057' }}>{user.institution.name}</span>
                </div>
                
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ color: '#6c757d', fontWeight: '500' }}>License Number:</span>
                  <span style={{ fontWeight: 'bold', color: '#495057' }}>{user.institution.licenseNumber}</span>
                </div>
                
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ color: '#6c757d', fontWeight: '500' }}>Business Phone:</span>
                  <span style={{ fontWeight: 'bold', color: '#495057' }}>{user.institution.businessPhoneNumber}</span>
                </div>
                
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ color: '#6c757d', fontWeight: '500' }}>Address:</span>
                  <span style={{ fontWeight: 'bold', color: '#495057' }}>{user.institution.address}</span>
                </div>
                
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ color: '#6c757d', fontWeight: '500' }}>License Issue Date:</span>
                  <span style={{ fontWeight: 'bold', color: '#495057' }}>
                    {new Date(user.institution.licenseIssueDate).toLocaleDateString('en-US')}
                  </span>
                </div>
                
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ color: '#6c757d', fontWeight: '500' }}>License Expiry Date:</span>
                  <span style={{ fontWeight: 'bold', color: '#495057' }}>
                    {new Date(user.institution.licenseExpiryDate).toLocaleDateString('en-US')}
                  </span>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Quick Actions */}
        <div style={{
          background: 'white',
          padding: '2rem',
          borderRadius: '15px',
          boxShadow: '0 8px 25px rgba(0,0,0,0.1)',
          border: '1px solid #e9ecef',
          marginBottom: '2rem'
        }}>
          <h3 style={{ margin: '0 0 1.5rem 0', color: '#243483' }}>
            <i className="fas fa-bolt" style={{ marginRight: '0.5rem' }}></i>
            Quick Actions
          </h3>
          
          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
            gap: '1rem'
          }}>
          
            
            <button
              onClick={() => navigate('/')}
              style={{
                background: 'linear-gradient(135deg, #243483 0%, #1467EA 100%)',
                color: 'white',
                border: 'none',
                padding: '1rem',
                borderRadius: '10px',
                cursor: 'pointer',
                fontWeight: 'bold',
                transition: 'all 0.3s ease'
              }}
              onMouseEnter={(e) => {
                e.target.style.transform = 'translateY(-2px)';
                e.target.style.boxShadow = '0 8px 25px rgba(36, 52, 131, 0.3)';
              }}
              onMouseLeave={(e) => {
                e.target.style.transform = 'translateY(0)';
                e.target.style.boxShadow = 'none';
              }}
            >
              <i className="fas fa-home" style={{ display: 'block', fontSize: '1.5rem', marginBottom: '0.5rem' ,width:'100%'}}></i>
              Home Page
            </button>
            
            <button
              onClick={handleLogout}
              style={{
                background: '#28a745 ',
                color: 'white',
                border: 'none',
                padding: '1rem',
                borderRadius: '10px',
                cursor: 'pointer',
                fontWeight: 'bold',
                transition: 'all 0.3s ease'
              }}
              onMouseEnter={(e) => {
                e.target.style.transform = 'translateY(-2px)';
                e.target.style.boxShadow = '0 8px 25px #28a745 ';
              }}
              onMouseLeave={(e) => {
                e.target.style.transform = 'translateY(0)';
                e.target.style.boxShadow = 'none';
              }}
            >
              <i className="fas fa-sign-out-alt" style={{ display: 'block', fontSize: '1.5rem', marginBottom: '0.5rem',width:'100%' }}></i>
              Logout
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
