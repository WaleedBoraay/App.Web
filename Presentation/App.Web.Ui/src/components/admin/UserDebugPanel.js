import React from 'react';
import { 
  getCurrentUser, 
  getCurrentUserId, 
  debugUserData, 
  createMockUser, 
  setupDevUser, 
  fixExistingUserData,
  logout 
} from '../../utils/userUtils';

const UserDebugPanel = () => {
  const handleDebugUser = () => {
    debugUserData();
  };

  const handleCreateMockUser = () => {
    createMockUser();
    window.location.reload(); // Reload to see changes
  };

  const handleSetupDevUser = () => {
    setupDevUser();
    window.location.reload(); // Reload to see changes
  };

  const handleFixUserData = () => {
    fixExistingUserData();
    window.location.reload(); // Reload to see changes
  };

  const handleLogout = () => {
    logout();
    window.location.reload(); // Reload to see changes
  };

  const handleSetUserRole = (role) => {
    const user = getCurrentUser();
    if (user) {
      const updatedUser = {
        ...user,
        roles: [role],
        role: role // For backward compatibility
      };
      localStorage.setItem('currentUser', JSON.stringify(updatedUser));
      console.log('User role updated:', updatedUser);
      window.location.reload(); // Reload to see changes
    }
  };

  const currentUser = getCurrentUser();
  const currentUserId = getCurrentUserId();

  return (
    <div style={{
      position: 'fixed',
      top: '10px',
      right: '10px',
      background: 'white',
      border: '2px solid #007bff',
      borderRadius: '8px',
      padding: '16px',
      boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
      zIndex: 9999,
      maxWidth: '300px',
      fontSize: '14px'
    }}>
      <h4 style={{ margin: '0 0 12px 0', color: '#007bff' }}>🔧 User Debug Panel</h4>
      
      <div style={{ marginBottom: '12px' }}>
        <strong>Current User:</strong><br />
        {currentUser ? (
          <div style={{ background: '#f8f9fa', padding: '8px', borderRadius: '4px', marginTop: '4px' }}>
            <div><strong>ID:</strong> {currentUserId || 'No ID'}</div>
            <div><strong>Username:</strong> {currentUser.username || 'No username'}</div>
            <div><strong>Email:</strong> {currentUser.email || 'No email'}</div>
            <div><strong>Role:</strong> {currentUser.role || currentUser.roles?.[0] || 'No role'}</div>
            <div><strong>Roles:</strong> {currentUser.roles ? JSON.stringify(currentUser.roles) : 'No roles'}</div>
          </div>
        ) : (
          <span style={{ color: '#dc3545' }}>No user logged in</span>
        )}
      </div>

      <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
        <button
          onClick={handleDebugUser}
          style={{
            background: '#17a2b8',
            color: 'white',
            border: 'none',
            padding: '6px 12px',
            borderRadius: '4px',
            cursor: 'pointer',
            fontSize: '12px'
          }}
        >
          🔍 Debug User Data
        </button>

        <button
          onClick={handleCreateMockUser}
          style={{
            background: '#28a745',
            color: 'white',
            border: 'none',
            padding: '6px 12px',
            borderRadius: '4px',
            cursor: 'pointer',
            fontSize: '12px'
          }}
        >
          👤 Create Mock User
        </button>

        <button
          onClick={handleSetupDevUser}
          style={{
            background: '#ffc107',
            color: '#000',
            border: 'none',
            padding: '6px 12px',
            borderRadius: '4px',
            cursor: 'pointer',
            fontSize: '12px'
          }}
        >
          ⚙️ Setup Dev User
        </button>

        {currentUser && (
          <button
            onClick={handleFixUserData}
            style={{
              background: '#fd7e14',
              color: 'white',
              border: 'none',
              padding: '6px 12px',
              borderRadius: '4px',
              cursor: 'pointer',
              fontSize: '12px'
            }}
          >
            🔧 Fix User ID to 4
          </button>
        )}

        {currentUser && (
          <>
            <div style={{ marginTop: '8px', marginBottom: '4px', fontSize: '12px', fontWeight: 'bold' }}>
              Set User Role:
            </div>
            <div style={{ display: 'flex', gap: '4px', flexWrap: 'wrap' }}>
              {['Maker', 'Checker', 'Regulator', 'Inspector', 'Admin'].map(role => (
                <button
                  key={role}
                  onClick={() => handleSetUserRole(role)}
                  style={{
                    background: currentUser.role === role || currentUser.roles?.includes(role) ? '#007bff' : '#6c757d',
                    color: 'white',
                    border: 'none',
                    padding: '4px 8px',
                    borderRadius: '3px',
                    cursor: 'pointer',
                    fontSize: '10px'
                  }}
                >
                  {role}
                </button>
              ))}
            </div>
            
            <button
              onClick={handleLogout}
              style={{
                background: '#dc3545',
                color: 'white',
                border: 'none',
                padding: '6px 12px',
                borderRadius: '4px',
                cursor: 'pointer',
                fontSize: '12px',
                marginTop: '8px'
              }}
            >
              🚪 Logout
            </button>
          </>
        )}
      </div>

      <div style={{ 
        marginTop: '12px', 
        fontSize: '11px', 
        color: '#6c757d',
        borderTop: '1px solid #e9ecef',
        paddingTop: '8px'
      }}>
        <strong>Debug Info:</strong><br />
        Check browser console for detailed logs
      </div>
    </div>
  );
};

export default UserDebugPanel;