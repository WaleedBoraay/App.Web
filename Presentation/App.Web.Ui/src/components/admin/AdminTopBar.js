import React from 'react';

const AdminTopBar = ({ 
  user, 
  isMobile, 
  isSidebarOpen, 
  onToggleSidebar, 
  onLogout 
}) => {
  return (
    <div style={{
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      flexWrap: 'wrap',
      gap: '1rem',
      marginBottom: '1.5rem',
      background: 'white',
      padding: isMobile ? '1rem' : '1rem 1.5rem',
      borderRadius: '12px',
      boxShadow: '0 2px 10px rgba(0,0,0,0.05)'
    }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
        <button
          onClick={onToggleSidebar}
          style={{
            background: isMobile ? 'linear-gradient(135deg, #243483 0%, #1467EA 100%)' : '#f8f9fa',
            border: 'none',
            borderRadius: '10px',
            padding: '0.75rem',
            cursor: 'pointer',
            color: isMobile ? 'white' : '#495057',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            width: '44px',
            height: '44px',
            transition: 'all 0.2s ease',
            boxShadow: isMobile ? '0 4px 12px rgba(36, 52, 131, 0.3)' : 'none'
          }}
          onMouseEnter={(e) => {
            if (!isMobile) {
              e.target.style.background = '#e9ecef';
              e.target.style.transform = 'scale(1.05)';
            }
          }}
          onMouseLeave={(e) => {
            if (!isMobile) {
              e.target.style.background = '#f8f9fa';
              e.target.style.transform = 'scale(1)';
            }
          }}
        >
          <i className={`fas fa-${isSidebarOpen ? 'times' : 'bars'}`}></i>
        </button>
        <div style={{ flex: isMobile ? '1 1 100%' : 'none' }}>
          <h1 style={{ 
            margin: 0, 
            color: '#243483', 
            fontSize: isMobile ? '1.25rem' : '1.5rem', 
            fontWeight: 700 
          }}>Admin Dashboard</h1>
          <p style={{ 
            margin: '0.25rem 0 0 0', 
            color: '#6c757d', 
            fontSize: isMobile ? '0.8rem' : '0.9rem' 
          }}>
            Welcome back, <strong>{user.username}</strong>
          </p>
        </div>
      </div>
      <div style={{ 
        display: 'flex', 
        alignItems: 'center', 
        gap: isMobile ? '0.5rem' : '1rem',
        flexWrap: 'wrap'
      }}>
        {user.roles && user.roles.length > 0 && (
          <span style={{
            display: 'inline-block',
            background: 'linear-gradient(135deg, #28a745 0%, #20c997 100%)',
            color: 'white',
            fontSize: isMobile ? '0.7rem' : '0.75rem',
            padding: isMobile ? '0.3rem 0.6rem' : '0.4rem 0.8rem',
            borderRadius: '20px',
            fontWeight: 600,
            textTransform: 'uppercase',
            letterSpacing: '0.5px'
          }}>
            {user.roles[0]}
          </span>
        )}
      </div>
    </div>
  );
};

export default AdminTopBar;
