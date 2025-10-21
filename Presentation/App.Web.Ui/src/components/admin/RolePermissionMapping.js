import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import { rolesApi, rolePermissionMappingApi } from '../../services/apiService';

const RolePermissionMapping = ({ isMobile }) => {
  const [roles, setRoles] = useState([]);
  const [selectedRole, setSelectedRole] = useState(null);
  const [rolePermissions, setRolePermissions] = useState(null);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [selectedPermissions, setSelectedPermissions] = useState([]);
  const [permissionCategories, setPermissionCategories] = useState({});

  // Load roles
  const loadRoles = async () => {
    setLoading(true);
    try {
      const response = await rolesApi.getAll();
      const activeRoles = (response || []).filter(role => role.isActive);
      setRoles(activeRoles);
      
      // Auto-select first role if available
      if (activeRoles.length > 0 && !selectedRole) {
        setSelectedRole(activeRoles[0]);
      }
      
      toast.success('Roles loaded successfully');
    } catch (error) {
      console.error('Error loading roles:', error);
      toast.error('Failed to load roles');
    } finally {
      setLoading(false);
    }
  };

  // Load permissions for selected role
  const loadRolePermissions = async (role) => {
    if (!role) return;
    
    setLoading(true);
    try {
      const response = await rolePermissionMappingApi.getRolePermissions(role.id);
      setRolePermissions(response);
      setSelectedPermissions(response.grantedPermissions || []);
      
      // Group permissions by category
      const categories = {};
      response.allPermissions?.forEach(permission => {
        if (!categories[permission.category]) {
          categories[permission.category] = [];
        }
        categories[permission.category].push(permission);
      });
      setPermissionCategories(categories);
      
      toast.success(`Permissions loaded for ${role.name}`);
    } catch (error) {
      console.error('Error loading role permissions:', error);
      toast.error('Failed to load role permissions');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadRoles();
  }, []);

  useEffect(() => {
    if (selectedRole) {
      loadRolePermissions(selectedRole);
    }
  }, [selectedRole]);

  // Handle role selection
  const handleRoleSelection = (role) => {
    setSelectedRole(role);
    setRolePermissions(null);
    setSelectedPermissions([]);
    setPermissionCategories({});
  };

  // Handle permission toggle
  const handlePermissionToggle = (permissionSystemName) => {
    setSelectedPermissions(prev => {
      const isSelected = prev.includes(permissionSystemName);
      if (isSelected) {
        return prev.filter(p => p !== permissionSystemName);
      } else {
        return [...prev, permissionSystemName];
      }
    });
  };

  // Save permission changes
  const handleSavePermissions = async () => {
    if (!selectedRole) return;

    setSaving(true);
    try {
      const response = await rolePermissionMappingApi.updateRolePermissions(
        selectedRole.id, 
        selectedPermissions
      );
      
      if (response.success) {
        toast.success(response.message || 'Role permissions updated successfully');
        // Reload permissions to reflect changes
        await loadRolePermissions(selectedRole);
      }
    } catch (error) {
      console.error('Error saving permissions:', error);
      toast.error('Failed to update role permissions');
    } finally {
      setSaving(false);
    }
  };

  // Check if there are unsaved changes
  const hasUnsavedChanges = () => {
    if (!rolePermissions) return false;
    const originalPermissions = rolePermissions.grantedPermissions || [];
    return JSON.stringify([...selectedPermissions].sort()) !== JSON.stringify([...originalPermissions].sort());
  };

  if (loading && roles.length === 0) {
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
        <p style={{ color: '#6c757d' }}>Loading roles...</p>
      </div>
    );
  }

  return (
    <div style={{ padding: isMobile ? '1rem' : '1.5rem' }}>
      {/* Header */}
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
            <i className="fas fa-shield-alt" style={{ marginRight: '0.5rem', color: '#20ABA0' }}></i>
            Role Permission Mapping
          </h2>
          <p style={{
            color: '#6c757d',
            fontSize: '0.95rem',
            margin: '0.5rem 0 0 0'
          }}>
            Configure permissions for each role
          </p>
        </div>

        {hasUnsavedChanges() && (
          <button
            onClick={handleSavePermissions}
            disabled={saving}
            style={{
              background: 'linear-gradient(135deg, #20ABA0 0%, #2CE86F 100%)',
              color: 'white',
              border: 'none',
              padding: '0.75rem 1.5rem',
              borderRadius: '8px',
              fontSize: '0.95rem',
              fontWeight: '600',
              cursor: saving ? 'not-allowed' : 'pointer',
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
              opacity: saving ? 0.7 : 1,
              transition: 'all 0.3s ease',
              width: isMobile ? '100%' : 'auto',
              justifyContent: 'center'
            }}
          >
            <i className="fas fa-save"></i>
            {saving ? 'Saving...' : 'Save Changes'}
          </button>
        )}
      </div>

      <div style={{
        display: 'grid',
        gridTemplateColumns: isMobile ? '1fr' : '300px 1fr',
        gap: '2rem',
        height: 'calc(100vh - 200px)',
        minHeight: '600px'
      }}>
        {/* Roles List */}
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
              Active Roles ({roles.length})
            </h4>
          </div>

          <div style={{
            maxHeight: isMobile ? '200px' : '500px',
            overflowY: 'auto'
          }}>
            {roles.map((role) => (
              <button
                key={role.id}
                onClick={() => handleRoleSelection(role)}
                disabled={loading}
                style={{
                  width: '100%',
                  padding: '1rem 1.5rem',
                  border: 'none',
                  borderBottom: '1px solid #f0f0f0',
                  backgroundColor: selectedRole?.id === role.id ? '#e3f2fd' : 'white',
                  cursor: loading ? 'not-allowed' : 'pointer',
                  textAlign: 'left',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '1rem',
                  transition: 'background-color 0.2s ease',
                  opacity: loading ? 0.6 : 1
                }}
                onMouseEnter={(e) => {
                  if (selectedRole?.id !== role.id && !loading) {
                    e.target.style.backgroundColor = '#f8f9fa';
                  }
                }}
                onMouseLeave={(e) => {
                  if (selectedRole?.id !== role.id && !loading) {
                    e.target.style.backgroundColor = 'white';
                  }
                }}
              >
                <div style={{
                  width: '40px',
                  height: '40px',
                  borderRadius: '50%',
                  background: selectedRole?.id === role.id 
                    ? 'linear-gradient(135deg, #20ABA0 0%, #2CE86F 100%)' 
                    : 'linear-gradient(135deg, #6c757d 0%, #495057 100%)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'white',
                  fontWeight: '600',
                  fontSize: '1rem'
                }}>
                  <i className={`fas fa-${role.isSystemRole ? 'shield-alt' : 'user-tag'}`}></i>
                </div>
                <div style={{ flex: 1 }}>
                  <div style={{
                    fontWeight: '600',
                    color: '#2c3e50',
                    marginBottom: '0.25rem'
                  }}>
                    {role.name}
                  </div>
                  <div style={{
                    fontSize: '0.8rem',
                    color: '#6c757d'
                  }}>
                    {role.isSystemRole ? 'System Role' : 'Custom Role'}
                  </div>
                </div>
                {selectedRole?.id === role.id && (
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
          {selectedRole && rolePermissions ? (
            <>
              <div style={{
                background: 'linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%)',
                padding: '1rem 1.5rem',
                borderBottom: '1px solid #dee2e6'
              }}>
                <div style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  flexDirection: isMobile ? 'column' : 'row',
                  gap: isMobile ? '0.5rem' : '0'
                }}>
                  <h4 style={{
                    color: '#495057',
                    margin: '0',
                    fontSize: '1.1rem',
                    fontWeight: '600'
                  }}>
                    <i className="fas fa-key" style={{ marginRight: '0.5rem' }}></i>
                    Permissions for {rolePermissions.roleName}
                  </h4>
                  <div style={{
                    fontSize: '0.85rem',
                    color: '#6c757d'
                  }}>
                    {selectedPermissions.length} of {rolePermissions.allPermissions?.length || 0} permissions selected
                  </div>
                </div>
              </div>

              <div style={{
                padding: '1.5rem',
                maxHeight: '500px',
                overflowY: 'auto'
              }}>
                {Object.keys(permissionCategories).length === 0 ? (
                  <div style={{
                    textAlign: 'center',
                    padding: '2rem',
                    color: '#6c757d'
                  }}>
                    <i className="fas fa-key" style={{ fontSize: '2rem', marginBottom: '1rem', color: '#dee2e6' }}></i>
                    <p>No permissions available for this role.</p>
                  </div>
                ) : (
                  Object.keys(permissionCategories).map(category => (
                    <div key={category} style={{ marginBottom: '2rem' }}>
                      <h5 style={{
                        color: '#495057',
                        fontSize: '1rem',
                        fontWeight: '600',
                        marginBottom: '1rem',
                        paddingBottom: '0.5rem',
                        borderBottom: '2px solid #e9ecef',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '0.5rem'
                      }}>
                        <i className="fas fa-folder" style={{ color: '#20ABA0' }}></i>
                        {category}
                        <span style={{
                          marginLeft: 'auto',
                          fontSize: '0.8rem',
                          fontWeight: '400',
                          color: '#6c757d'
                        }}>
                          {permissionCategories[category].filter(p => selectedPermissions.includes(p.systemName)).length}/{permissionCategories[category].length}
                        </span>
                      </h5>

                      <div style={{
                        display: 'grid',
                        gridTemplateColumns: isMobile ? '1fr' : 'repeat(auto-fill, minmax(300px, 1fr))',
                        gap: '1rem'
                      }}>
                        {permissionCategories[category].map(permission => {
                          const isSelected = selectedPermissions.includes(permission.systemName);
                          return (
                            <div
                              key={permission.id}
                              style={{
                                padding: '1rem',
                                border: `2px solid ${isSelected ? '#20ABA0' : '#e9ecef'}`,
                                borderRadius: '8px',
                                backgroundColor: isSelected ? '#f0fffe' : '#fafafa',
                                cursor: 'pointer',
                                transition: 'all 0.2s ease'
                              }}
                              onClick={() => handlePermissionToggle(permission.systemName)}
                            >
                              <div style={{
                                display: 'flex',
                                alignItems: 'flex-start',
                                gap: '1rem'
                              }}>
                                <input
                                  type="checkbox"
                                  checked={isSelected}
                                  onChange={() => handlePermissionToggle(permission.systemName)}
                                  style={{
                                    width: '18px',
                                    height: '18px',
                                    marginTop: '2px',
                                    accentColor: '#20ABA0'
                                  }}
                                />
                                <div style={{ flex: 1 }}>
                                  <h6 style={{
                                    margin: '0 0 0.5rem 0',
                                    color: '#2c3e50',
                                    fontSize: '0.95rem',
                                    fontWeight: '600'
                                  }}>
                                    {permission.name}
                                  </h6>
                                  <p style={{
                                    margin: '0 0 0.5rem 0',
                                    color: '#495057',
                                    fontSize: '0.85rem',
                                    lineHeight: '1.4'
                                  }}>
                                    {permission.description}
                                  </p>
                                  <code style={{
                                    fontSize: '0.75rem',
                                    color: '#6c757d',
                                    backgroundColor: '#f8f9fa',
                                    padding: '0.25rem 0.5rem',
                                    borderRadius: '4px',
                                    border: '1px solid #e9ecef'
                                  }}>
                                    {permission.systemName}
                                  </code>
                                </div>
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    </div>
                  ))
                )}
              </div>
            </>
          ) : selectedRole && loading ? (
            <div style={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
              height: '100%',
              padding: '3rem'
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
              <p style={{ color: '#6c757d' }}>Loading permissions...</p>
            </div>
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
              <i className="fas fa-shield-alt" style={{ fontSize: '4rem', marginBottom: '1rem', color: '#dee2e6' }}></i>
              <h3 style={{ marginBottom: '0.5rem' }}>Select a Role</h3>
              <p style={{ textAlign: 'center' }}>
                Choose a role from the list to configure its permissions
              </p>
            </div>
          )}
        </div>
      </div>

      {/* Changes indicator */}
      {hasUnsavedChanges() && (
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
            You have unsaved permission changes. Click "Save Changes" to apply them.
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

export default RolePermissionMapping;