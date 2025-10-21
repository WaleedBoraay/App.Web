import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import { userRolesApi } from '../../services/apiService';

const UserRoles = ({ isMobile }) => {
  const [users, setUsers] = useState([]);
  const [allRoles, setAllRoles] = useState([]);
  const [loading, setLoading] = useState(false);
  const [selectedUserRoles, setSelectedUserRoles] = useState({});

  // Load user roles data
  const loadUserRoles = async () => {
    setLoading(true);
    try {
      const response = await userRolesApi.getAll();
      setUsers(response.users || []);
      setAllRoles(response.allRoles || []);
      
      // Initialize selected roles state
      const roleState = {};
      response.users?.forEach(user => {
        roleState[user.userId] = user.roles || [];
      });
      setSelectedUserRoles(roleState);
      
      toast.success('User roles loaded successfully');
    } catch (error) {
      console.error('Error loading user roles:', error);
      toast.error('Failed to load user roles');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUserRoles();
  }, []);

  // Handle role selection change
  const handleRoleChange = (userId, roleName, isChecked) => {
    setSelectedUserRoles(prev => ({
      ...prev,
      [userId]: isChecked 
        ? [...(prev[userId] || []), roleName]
        : (prev[userId] || []).filter(role => role !== roleName)
    }));
  };

  // Save role changes
  const handleSaveChanges = async () => {
    setLoading(true);
    try {
      // Convert selected roles back to role IDs for API
      const userRoles = Object.keys(selectedUserRoles).map(userId => ({
        userId: parseInt(userId),
        selectedRoleIds: selectedUserRoles[userId].map(roleName => {
          const role = allRoles.find(r => r.name === roleName);
          return role ? role.id : null;
        }).filter(id => id !== null)
      }));

      await userRolesApi.updateRoles(userRoles);
      toast.success('User roles updated successfully');
      await loadUserRoles(); // Refresh data
    } catch (error) {
      console.error('Error updating user roles:', error);
      toast.error('Failed to update user roles');
    } finally {
      setLoading(false);
    }
  };

  // Check if there are any changes
  const hasChanges = () => {
    return users.some(user => {
      const currentRoles = selectedUserRoles[user.userId] || [];
      const originalRoles = user.roles || [];
      return JSON.stringify(currentRoles.sort()) !== JSON.stringify(originalRoles.sort());
    });
  };

  if (loading) {
    return (
      <div style={{ 
        display: 'flex', 
        justifyContent: 'center', 
        alignItems: 'center', 
        minHeight: '400px',
        flexDirection: 'column'
      }}>
        <div style={{
          border: '3px solid #f3f3f3',
          borderTop: '3px solid #20ABA0',
          borderRadius: '50%',
          width: '40px',
          height: '40px',
          animation: 'spin 1s linear infinite',
          marginBottom: '1rem'
        }}></div>
        <p style={{ color: '#6c757d' }}>Loading user roles...</p>
      </div>
    );
  }

  return (
    <div style={{ padding: isMobile ? '1rem' : '1.5rem' }}>
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: '2rem',
        flexDirection: isMobile ? 'column' : 'row',
        gap: isMobile ? '1rem' : '0'
      }}>
        <div>
          <h2 style={{
            color: '#2c3e50',
            fontSize: isMobile ? '1.5rem' : '1.8rem',
            fontWeight: '600',
            margin: '0'
          }}>
            <i className="fas fa-users-cog" style={{ marginRight: '0.5rem', color: '#20ABA0' }}></i>
            User Role Management
          </h2>
          <p style={{
            color: '#6c757d',
            fontSize: '0.95rem',
            margin: '0.5rem 0 0 0'
          }}>
            Assign and manage user roles and permissions
          </p>
        </div>

        {hasChanges() && (
          <button
            onClick={handleSaveChanges}
            disabled={loading}
            style={{
              background: 'linear-gradient(135deg, #20ABA0 0%, #2CE86F 100%)',
              color: 'white',
              border: 'none',
              padding: '0.75rem 1.5rem',
              borderRadius: '8px',
              fontSize: '0.95rem',
              fontWeight: '600',
              cursor: loading ? 'not-allowed' : 'pointer',
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
              opacity: loading ? 0.7 : 1,
              transition: 'all 0.3s ease',
              width: isMobile ? '100%' : 'auto',
              justifyContent: 'center'
            }}
          >
            <i className="fas fa-save"></i>
            {loading ? 'Saving...' : 'Save Changes'}
          </button>
        )}
      </div>

      {users.length === 0 ? (
        <div style={{
          textAlign: 'center',
          padding: '3rem',
          color: '#6c757d',
          backgroundColor: '#f8f9fa',
          borderRadius: '12px',
          border: '2px dashed #dee2e6'
        }}>
          <i className="fas fa-users" style={{ fontSize: '3rem', marginBottom: '1rem', color: '#dee2e6' }}></i>
          <h3 style={{ marginBottom: '0.5rem' }}>No Users Found</h3>
          <p>No users available for role assignment.</p>
        </div>
      ) : (
        <div style={{
          backgroundColor: 'white',
          borderRadius: '12px',
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          border: '1px solid #f0f0f0',
          overflow: 'hidden'
        }}>
          {/* Header */}
          <div style={{
            background: 'linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%)',
            padding: '1rem 1.5rem',
            borderBottom: '1px solid #dee2e6',
            display: isMobile ? 'block' : 'flex',
            alignItems: 'center',
            justifyContent: 'space-between'
          }}>
            <h4 style={{
              color: '#495057',
              margin: '0',
              fontSize: '1.1rem',
              fontWeight: '600'
            }}>
              Users and Role Assignments
            </h4>
            <span style={{
              color: '#6c757d',
              fontSize: '0.9rem',
              marginTop: isMobile ? '0.5rem' : '0',
              display: 'block'
            }}>
              {users.length} user{users.length !== 1 ? 's' : ''}
            </span>
          </div>

          {/* Users List */}
          <div style={{ 
            maxHeight: '600px', 
            overflowY: 'auto',
            padding: '0'
          }}>
            {users.map((user, index) => (
              <div 
                key={user.userId}
                style={{
                  padding: '1.5rem',
                  borderBottom: index < users.length - 1 ? '1px solid #f0f0f0' : 'none',
                  backgroundColor: index % 2 === 0 ? '#ffffff' : '#fafafa'
                }}
              >
                <div style={{
                  display: 'flex',
                  alignItems: 'flex-start',
                  gap: '1rem',
                  flexDirection: isMobile ? 'column' : 'row'
                }}>
                  {/* User Info */}
                  <div style={{
                    minWidth: isMobile ? '100%' : '200px',
                    marginBottom: isMobile ? '1rem' : '0'
                  }}>
                    <div style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: '0.75rem',
                      marginBottom: '0.5rem'
                    }}>
                      <div style={{
                        width: '40px',
                        height: '40px',
                        borderRadius: '50%',
                        background: 'linear-gradient(135deg, #20ABA0 0%, #2CE86F 100%)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        color: 'white',
                        fontWeight: '600',
                        fontSize: '1.1rem'
                      }}>
                        {user.username.charAt(0).toUpperCase()}
                      </div>
                      <div>
                        <h5 style={{
                          color: '#2c3e50',
                          margin: '0',
                          fontSize: '1.1rem',
                          fontWeight: '600'
                        }}>
                          {user.username}
                        </h5>
                        <p style={{
                          color: '#6c757d',
                          margin: '0',
                          fontSize: '0.85rem'
                        }}>
                          User ID: {user.userId}
                        </p>
                      </div>
                    </div>
                    <div style={{
                      fontSize: '0.85rem',
                      color: '#495057'
                    }}>
                      <strong>Current Roles:</strong> {(selectedUserRoles[user.userId] || []).length || 'None'}
                    </div>
                  </div>

                  {/* Role Checkboxes */}
                  <div style={{
                    flex: 1,
                    display: 'grid',
                    gridTemplateColumns: isMobile ? '1fr' : 'repeat(auto-fit, minmax(200px, 1fr))',
                    gap: '0.75rem'
                  }}>
                    {allRoles.map(role => {
                      const isChecked = (selectedUserRoles[user.userId] || []).includes(role.name);
                      return (
                        <label 
                          key={role.id}
                          style={{
                            display: 'flex',
                            alignItems: 'center',
                            gap: '0.5rem',
                            cursor: 'pointer',
                            padding: '0.5rem',
                            borderRadius: '6px',
                            backgroundColor: isChecked ? '#e8f5e8' : '#f8f9fa',
                            border: `1px solid ${isChecked ? '#20ABA0' : '#dee2e6'}`,
                            transition: 'all 0.2s ease'
                          }}
                        >
                          <input
                            type="checkbox"
                            checked={isChecked}
                            onChange={(e) => handleRoleChange(user.userId, role.name, e.target.checked)}
                            style={{
                              width: '16px',
                              height: '16px',
                              accentColor: '#20ABA0'
                            }}
                          />
                          <span style={{
                            fontSize: '0.9rem',
                            color: isChecked ? '#155724' : '#495057',
                            fontWeight: isChecked ? '600' : '400'
                          }}>
                            {role.name}
                          </span>
                        </label>
                      );
                    })}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Changes indicator */}
      {hasChanges() && (
        <div style={{
          marginTop: '1rem',
          padding: '1rem',
          backgroundColor: '#fff3cd',
          border: '1px solid #ffeaa7',
          borderRadius: '8px',
          display: 'flex',
          alignItems: 'center',
          gap: '0.5rem'
        }}>
          <i className="fas fa-exclamation-triangle" style={{ color: '#856404' }}></i>
          <span style={{ color: '#856404', fontSize: '0.9rem' }}>
            You have unsaved changes. Click "Save Changes" to apply them.
          </span>
        </div>
      )}

      <style jsx>{`
        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }
      `}</style>
    </div>
  );
};

export default UserRoles;