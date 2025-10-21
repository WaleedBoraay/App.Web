import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import { userPermissionOverridesApi } from '../../services/apiService';

const UserPermissionOverrides = ({ isMobile }) => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [selectedUser, setSelectedUser] = useState(null);
  const [permissionCategories, setPermissionCategories] = useState({});

  // Load user permission overrides data
  const loadUserPermissionOverrides = async () => {
    setLoading(true);
    try {
      const response = await userPermissionOverridesApi.getAll();
      setUsers(response || []);
      
      // Group permissions by category
      if (response && response.length > 0) {
        const categories = {};
        response[0].allPermissions?.forEach(permission => {
          if (!categories[permission.category]) {
            categories[permission.category] = [];
          }
          categories[permission.category].push(permission);
        });
        setPermissionCategories(categories);
      }
      
      // Set first user as selected by default
      if (response && response.length > 0) {
        setSelectedUser(response[0]);
      }
      
      toast.success('User permission overrides loaded successfully');
    } catch (error) {
      console.error('Error loading user permission overrides:', error);
      toast.error('Failed to load user permission overrides');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUserPermissionOverrides();
  }, []);

  // Get permission status for selected user
  const getPermissionStatus = (permissionSystemName) => {
    if (!selectedUser) return 'neutral';
    
    if (selectedUser.granted?.includes(permissionSystemName)) {
      return 'granted';
    }
    if (selectedUser.denied?.includes(permissionSystemName)) {
      return 'denied';
    }
    return 'neutral';
  };

  // Handle permission action (grant, deny, remove)
  const handlePermissionAction = async (permissionSystemName, action) => {
    if (!selectedUser) return;

    setLoading(true);
    try {
      let response;
      switch (action) {
        case 'grant':
          response = await userPermissionOverridesApi.grantPermission(selectedUser.userId, permissionSystemName);
          break;
        case 'deny':
          response = await userPermissionOverridesApi.denyPermission(selectedUser.userId, permissionSystemName);
          break;
        case 'remove':
          response = await userPermissionOverridesApi.removeOverride(selectedUser.userId, permissionSystemName);
          break;
        default:
          return;
      }

      if (response.success) {
        toast.success(`Permission ${action === 'remove' ? 'override removed' : `${action}ed`} successfully`);
        await loadUserPermissionOverrides(); // Refresh data
        
        // Re-select the same user
        const updatedUser = users.find(u => u.userId === selectedUser.userId);
        if (updatedUser) {
          setSelectedUser(updatedUser);
        }
      }
    } catch (error) {
      console.error(`Error ${action}ing permission:`, error);
      toast.error(`Failed to ${action} permission`);
    } finally {
      setLoading(false);
    }
  };

  // Get status badge style
  const getStatusBadgeStyle = (status) => {
    const baseStyle = {
      padding: '0.25rem 0.75rem',
      borderRadius: '20px',
      fontSize: '0.75rem',
      fontWeight: '600',
      textTransform: 'uppercase',
      letterSpacing: '0.5px'
    };

    switch (status) {
      case 'granted':
        return {
          ...baseStyle,
          backgroundColor: '#d4edda',
          color: '#155724',
          border: '1px solid #c3e6cb'
        };
      case 'denied':
        return {
          ...baseStyle,
          backgroundColor: '#f8d7da',
          color: '#721c24',
          border: '1px solid #f5c6cb'
        };
      default:
        return {
          ...baseStyle,
          backgroundColor: '#e9ecef',
          color: '#495057',
          border: '1px solid #ced4da'
        };
    }
  };

  if (loading && users.length === 0) {
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
        <p style={{ color: '#6c757d' }}>Loading user permission overrides...</p>
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
            <i className="fas fa-user-lock" style={{ marginRight: '0.5rem', color: '#20ABA0' }}></i>
            User Permission Overrides
          </h2>
          <p style={{
            color: '#6c757d',
            fontSize: '0.95rem',
            margin: '0.5rem 0 0 0'
          }}>
            Manage specific permission overrides for individual users
          </p>
        </div>

        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '1rem',
          fontSize: '0.9rem',
          color: '#6c757d'
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <div style={{
              width: '12px',
              height: '12px',
              backgroundColor: '#d4edda',
              border: '1px solid #c3e6cb',
              borderRadius: '3px'
            }}></div>
            <span>Granted</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <div style={{
              width: '12px',
              height: '12px',
              backgroundColor: '#f8d7da',
              border: '1px solid #f5c6cb',
              borderRadius: '3px'
            }}></div>
            <span>Denied</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <div style={{
              width: '12px',
              height: '12px',
              backgroundColor: '#e9ecef',
              border: '1px solid #ced4da',
              borderRadius: '3px'
            }}></div>
            <span>Default</span>
          </div>
        </div>
      </div>

      <div style={{
        display: 'grid',
        gridTemplateColumns: isMobile ? '1fr' : '300px 1fr',
        gap: '2rem',
        height: 'calc(100vh - 200px)',
        minHeight: '600px'
      }}>
        {/* Users List */}
        <div style={{
          backgroundColor: 'white',
          borderRadius: '12px',
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          border: '1px solid #f0f0f0',
          overflow: 'hidden',
          height: 'fit-content',
          maxHeight: isMobile ? '300px' : '100%'
        }}>
          <div style={{
            background: 'linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%)',
            padding: '1rem 1.5rem',
            borderBottom: '1px solid #dee2e6'
          }}>
            <h4 style={{
              color: '#495057',
              margin: '0',
              fontSize: '1.1rem',
              fontWeight: '600'
            }}>
              <i className="fas fa-users" style={{ marginRight: '0.5rem' }}></i>
              Users ({users.length})
            </h4>
          </div>

          <div style={{
            maxHeight: isMobile ? '200px' : '500px',
            overflowY: 'auto'
          }}>
            {users.map((user) => (
              <button
                key={user.userId}
                onClick={() => setSelectedUser(user)}
                style={{
                  width: '100%',
                  padding: '1rem 1.5rem',
                  border: 'none',
                  borderBottom: '1px solid #f0f0f0',
                  backgroundColor: selectedUser?.userId === user.userId ? '#e3f2fd' : 'white',
                  cursor: 'pointer',
                  textAlign: 'left',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '1rem',
                  transition: 'background-color 0.2s ease'
                }}
                onMouseEnter={(e) => {
                  if (selectedUser?.userId !== user.userId) {
                    e.target.style.backgroundColor = '#f8f9fa';
                  }
                }}
                onMouseLeave={(e) => {
                  if (selectedUser?.userId !== user.userId) {
                    e.target.style.backgroundColor = 'white';
                  }
                }}
              >
                <div style={{
                  width: '40px',
                  height: '40px',
                  borderRadius: '50%',
                  background: selectedUser?.userId === user.userId 
                    ? 'linear-gradient(135deg, #20ABA0 0%, #2CE86F 100%)' 
                    : 'linear-gradient(135deg, #6c757d 0%, #495057 100%)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'white',
                  fontWeight: '600',
                  fontSize: '1rem'
                }}>
                  {user.username.charAt(0).toUpperCase()}
                </div>
                <div style={{ flex: 1 }}>
                  <div style={{
                    fontWeight: '600',
                    color: '#2c3e50',
                    marginBottom: '0.25rem'
                  }}>
                    {user.username}
                  </div>
                  <div style={{
                    fontSize: '0.8rem',
                    color: '#6c757d'
                  }}>
                    {user.granted?.length || 0} granted, {user.denied?.length || 0} denied
                  </div>
                </div>
                {selectedUser?.userId === user.userId && (
                  <i className="fas fa-chevron-right" style={{ color: '#20ABA0' }}></i>
                )}
              </button>
            ))}
          </div>
        </div>

        {/* Permissions Grid */}
        <div style={{
          backgroundColor: 'white',
          borderRadius: '12px',
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          border: '1px solid #f0f0f0',
          overflow: 'hidden'
        }}>
          {selectedUser ? (
            <>
              <div style={{
                background: 'linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%)',
                padding: '1rem 1.5rem',
                borderBottom: '1px solid #dee2e6'
              }}>
                <h4 style={{
                  color: '#495057',
                  margin: '0',
                  fontSize: '1.1rem',
                  fontWeight: '600'
                }}>
                  <i className="fas fa-key" style={{ marginRight: '0.5rem' }}></i>
                  Permissions for {selectedUser.username}
                </h4>
                <p style={{
                  color: '#6c757d',
                  fontSize: '0.85rem',
                  margin: '0.5rem 0 0 0'
                }}>
                  Click buttons to grant, deny, or remove permission overrides
                </p>
              </div>

              <div style={{
                padding: '1.5rem',
                maxHeight: '500px',
                overflowY: 'auto'
              }}>
                {Object.keys(permissionCategories).map(category => (
                  <div key={category} style={{ marginBottom: '2rem' }}>
                    <h5 style={{
                      color: '#495057',
                      fontSize: '1rem',
                      fontWeight: '600',
                      marginBottom: '1rem',
                      paddingBottom: '0.5rem',
                      borderBottom: '2px solid #e9ecef'
                    }}>
                      <i className="fas fa-folder" style={{ marginRight: '0.5rem', color: '#20ABA0' }}></i>
                      {category}
                    </h5>

                    <div style={{
                      display: 'grid',
                      gridTemplateColumns: '1fr',
                      gap: '1rem'
                    }}>
                      {permissionCategories[category].map(permission => {
                        const status = getPermissionStatus(permission.systemName);
                        return (
                          <div
                            key={permission.id}
                            style={{
                              padding: '1rem',
                              border: '1px solid #e9ecef',
                              borderRadius: '8px',
                              backgroundColor: '#fafafa'
                            }}
                          >
                            <div style={{
                              display: 'flex',
                              justifyContent: 'space-between',
                              alignItems: 'flex-start',
                              flexDirection: isMobile ? 'column' : 'row',
                              gap: '1rem'
                            }}>
                              <div style={{ flex: 1 }}>
                                <div style={{
                                  display: 'flex',
                                  alignItems: 'center',
                                  gap: '0.75rem',
                                  marginBottom: '0.5rem'
                                }}>
                                  <h6 style={{
                                    margin: '0',
                                    color: '#2c3e50',
                                    fontSize: '0.95rem',
                                    fontWeight: '600'
                                  }}>
                                    {permission.name}
                                  </h6>
                                  <span style={getStatusBadgeStyle(status)}>
                                    {status === 'neutral' ? 'Default' : status}
                                  </span>
                                </div>
                                <p style={{
                                  margin: '0',
                                  color: '#6c757d',
                                  fontSize: '0.85rem',
                                  lineHeight: '1.4'
                                }}>
                                  {permission.description}
                                </p>
                                <p style={{
                                  margin: '0.25rem 0 0 0',
                                  color: '#8e9297',
                                  fontSize: '0.75rem',
                                  fontFamily: 'monospace'
                                }}>
                                  {permission.systemName}
                                </p>
                              </div>

                              <div style={{
                                display: 'flex',
                                gap: '0.5rem',
                                width: isMobile ? '100%' : 'auto'
                              }}>
                                <button
                                  onClick={() => handlePermissionAction(permission.systemName, 'grant')}
                                  disabled={loading || status === 'granted'}
                                  style={{
                                    padding: '0.5rem 1rem',
                                    border: 'none',
                                    borderRadius: '6px',
                                    fontSize: '0.8rem',
                                    fontWeight: '600',
                                    cursor: loading || status === 'granted' ? 'not-allowed' : 'pointer',
                                    backgroundColor: status === 'granted' ? '#28a745' : '#20ABA0',
                                    color: 'white',
                                    opacity: loading || status === 'granted' ? 0.6 : 1,
                                    transition: 'all 0.2s ease',
                                    flex: isMobile ? '1' : 'auto'
                                  }}
                                >
                                  <i className="fas fa-check" style={{ marginRight: '0.25rem' }}></i>
                                  Grant
                                </button>

                                <button
                                  onClick={() => handlePermissionAction(permission.systemName, 'deny')}
                                  disabled={loading || status === 'denied'}
                                  style={{
                                    padding: '0.5rem 1rem',
                                    border: 'none',
                                    borderRadius: '6px',
                                    fontSize: '0.8rem',
                                    fontWeight: '600',
                                    cursor: loading || status === 'denied' ? 'not-allowed' : 'pointer',
                                    backgroundColor: status === 'denied' ? '#dc3545' : '#e74c3c',
                                    color: 'white',
                                    opacity: loading || status === 'denied' ? 0.6 : 1,
                                    transition: 'all 0.2s ease',
                                    flex: isMobile ? '1' : 'auto'
                                  }}
                                >
                                  <i className="fas fa-times" style={{ marginRight: '0.25rem' }}></i>
                                  Deny
                                </button>

                                {status !== 'neutral' && (
                                  <button
                                    onClick={() => handlePermissionAction(permission.systemName, 'remove')}
                                    disabled={loading}
                                    style={{
                                      padding: '0.5rem 1rem',
                                      border: '1px solid #6c757d',
                                      borderRadius: '6px',
                                      fontSize: '0.8rem',
                                      fontWeight: '600',
                                      cursor: loading ? 'not-allowed' : 'pointer',
                                      backgroundColor: 'white',
                                      color: '#6c757d',
                                      opacity: loading ? 0.6 : 1,
                                      transition: 'all 0.2s ease',
                                      flex: isMobile ? '1' : 'auto'
                                    }}
                                  >
                                    <i className="fas fa-undo" style={{ marginRight: '0.25rem' }}></i>
                                    Reset
                                  </button>
                                )}
                              </div>
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                ))}
              </div>
            </>
          ) : (
            <div style={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
              height: '100%',
              padding: '3rem',
              color: '#6c757d'
            }}>
              <i className="fas fa-user-lock" style={{ fontSize: '4rem', marginBottom: '1rem', color: '#dee2e6' }}></i>
              <h3 style={{ marginBottom: '0.5rem' }}>Select a User</h3>
              <p style={{ textAlign: 'center' }}>
                Choose a user from the list to manage their permission overrides
              </p>
            </div>
          )}
        </div>
      </div>

      <style jsx>{`
        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }
      `}</style>
    </div>
  );
};

export default UserPermissionOverrides;