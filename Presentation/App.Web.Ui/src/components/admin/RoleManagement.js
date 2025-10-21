import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import Modal from 'react-modal';
import { rolesApi } from '../../services/apiService';

const RoleManagement = ({ isMobile }) => {
  const [roles, setRoles] = useState([]);
  const [loading, setLoading] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState('add'); // 'add', 'edit', 'view'
  const [selectedRole, setSelectedRole] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('all'); // 'all', 'active', 'inactive'

  // Form data state
  const [formData, setFormData] = useState({
    name: '',
    systemName: '',
    description: '',
    isActive: true,
    isSystemRole: false
  });

  // Load roles
  const loadRoles = async () => {
    setLoading(true);
    try {
      const response = await rolesApi.getAll();
      setRoles(response || []);
      toast.success('Roles loaded successfully');
    } catch (error) {
      console.error('Error loading roles:', error);
      toast.error('Failed to load roles');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadRoles();
  }, []);

  // Filter roles based on search and status
  const filteredRoles = roles.filter(role => {
    const matchesSearch = role.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         role.systemName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         role.description.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesStatus = statusFilter === 'all' || 
                         (statusFilter === 'active' && role.isActive) ||
                         (statusFilter === 'inactive' && !role.isActive);

    return matchesSearch && matchesStatus;
  });

  // Open modal for different actions
  const openModal = (mode, role = null) => {
    setModalMode(mode);
    setSelectedRole(role);
    
    if (mode === 'add') {
      setFormData({
        name: '',
        systemName: '',
        description: '',
        isActive: true,
        isSystemRole: false
      });
    } else if (role) {
      setFormData({
        name: role.name || '',
        systemName: role.systemName || '',
        description: role.description || '',
        isActive: role.isActive || false,
        isSystemRole: role.isSystemRole || false
      });
    }
    
    setIsModalOpen(true);
  };

  // Close modal
  const closeModal = () => {
    setIsModalOpen(false);
    setSelectedRole(null);
    setFormData({
      name: '',
      systemName: '',
      description: '',
      isActive: true,
      isSystemRole: false
    });
  };

  // Handle form input changes
  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));

    // Auto-generate system name from name (only when adding)
    if (name === 'name' && modalMode === 'add') {
      const systemName = value.replace(/[^a-zA-Z0-9]/g, '').replace(/\s+/g, '');
      setFormData(prev => ({
        ...prev,
        systemName: systemName
      }));
    }
  };

  // Handle form submission
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!formData.name.trim() || !formData.systemName.trim()) {
      toast.error('Name and System Name are required');
      return;
    }

    setLoading(true);
    try {
      let response;
      if (modalMode === 'add') {
        response = await rolesApi.create(formData);
        toast.success('Role created successfully');
      } else if (modalMode === 'edit') {
        response = await rolesApi.update(selectedRole.id, formData);
        toast.success('Role updated successfully');
      }

      if (response.success) {
        await loadRoles();
        closeModal();
      }
    } catch (error) {
      console.error('Error saving role:', error);
      toast.error(`Failed to ${modalMode === 'add' ? 'create' : 'update'} role`);
    } finally {
      setLoading(false);
    }
  };

  // Handle role deletion
  const handleDelete = async (role) => {
    if (role.isSystemRole) {
      toast.error('System roles cannot be deleted');
      return;
    }

    if (window.confirm(`Are you sure you want to delete the role "${role.name}"?`)) {
      setLoading(true);
      try {
        const response = await rolesApi.delete(role.id);
        if (response.success) {
          toast.success('Role deleted successfully');
          await loadRoles();
        }
      } catch (error) {
        console.error('Error deleting role:', error);
        toast.error('Failed to delete role');
      } finally {
        setLoading(false);
      }
    }
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
            <i className="fas fa-user-shield" style={{ marginRight: '0.5rem', color: '#20ABA0' }}></i>
            Role Management
          </h2>
          <p style={{
            color: '#6c757d',
            fontSize: '0.95rem',
            margin: '0.5rem 0 0 0'
          }}>
            Manage system roles and their permissions
          </p>
        </div>

        <button
          onClick={() => openModal('add')}
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
          <i className="fas fa-plus"></i>
          Add New Role
        </button>
      </div>

      {/* Filters */}
      <div style={{
        display: 'flex',
        gap: '1rem',
        marginBottom: '2rem',
        flexDirection: isMobile ? 'column' : 'row',
        alignItems: isMobile ? 'stretch' : 'center'
      }}>
        <div style={{ flex: 1, minWidth: '200px' }}>
          <input
            type="text"
            placeholder="Search roles..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{
              width: '100%',
              padding: '0.75rem 1rem',
              border: '1px solid #ddd',
              borderRadius: '8px',
              fontSize: '0.95rem'
            }}
          />
        </div>
        
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          style={{
            padding: '0.75rem 1rem',
            border: '1px solid #ddd',
            borderRadius: '8px',
            fontSize: '0.95rem',
            minWidth: '150px'
          }}
        >
          <option value="all">All Roles</option>
          <option value="active">Active Only</option>
          <option value="inactive">Inactive Only</option>
        </select>
      </div>

      {/* Roles Table */}
      {filteredRoles.length === 0 ? (
        <div style={{
          textAlign: 'center',
          padding: '3rem',
          color: '#6c757d',
          backgroundColor: '#f8f9fa',
          borderRadius: '12px',
          border: '2px dashed #dee2e6'
        }}>
          <i className="fas fa-user-shield" style={{ fontSize: '3rem', marginBottom: '1rem', color: '#dee2e6' }}></i>
          <h3 style={{ marginBottom: '0.5rem' }}>No Roles Found</h3>
          <p>
            {searchTerm || statusFilter !== 'all' 
              ? 'No roles match your current filters.' 
              : 'No roles have been created yet.'
            }
          </p>
        </div>
      ) : (
        <div style={{
          backgroundColor: 'white',
          borderRadius: '12px',
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
          border: '1px solid #f0f0f0',
          overflow: 'hidden'
        }}>
          {/* Table Header */}
          <div style={{
            background: 'linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%)',
            padding: '1rem 1.5rem',
            borderBottom: '1px solid #dee2e6',
            display: 'grid',
            gridTemplateColumns: isMobile ? '1fr' : '2fr 1.5fr 1fr 1fr 150px',
            gap: '1rem',
            alignItems: 'center',
            fontWeight: '600',
            color: '#495057'
          }}>
            <span>Role Name</span>
            {!isMobile && <span>Description</span>}
            {!isMobile && <span>Status</span>}
            {!isMobile && <span>Type</span>}
            <span>Actions</span>
          </div>

          {/* Table Body */}
          <div style={{ maxHeight: '500px', overflowY: 'auto' }}>
            {filteredRoles.map((role, index) => (
              <div
                key={role.id}
                style={{
                  padding: '1rem 1.5rem',
                  borderBottom: index < filteredRoles.length - 1 ? '1px solid #f0f0f0' : 'none',
                  display: 'grid',
                  gridTemplateColumns: isMobile ? '1fr' : '2fr 1.5fr 1fr 1fr 150px',
                  gap: '1rem',
                  alignItems: 'center',
                  backgroundColor: index % 2 === 0 ? '#ffffff' : '#fafafa'
                }}
              >
                {/* Role Name & System Name */}
                <div>
                  <div style={{
                    fontWeight: '600',
                    color: '#2c3e50',
                    marginBottom: '0.25rem'
                  }}>
                    {role.name}
                  </div>
                  <div style={{
                    fontSize: '0.8rem',
                    color: '#6c757d',
                    fontFamily: 'monospace'
                  }}>
                    {role.systemName}
                  </div>
                  {isMobile && (
                    <div style={{
                      fontSize: '0.85rem',
                      color: '#495057',
                      marginTop: '0.5rem'
                    }}>
                      {role.description}
                    </div>
                  )}
                </div>

                {/* Description (Desktop only) */}
                {!isMobile && (
                  <div style={{
                    fontSize: '0.9rem',
                    color: '#495057',
                    lineHeight: '1.4'
                  }}>
                    {role.description}
                  </div>
                )}

                {/* Status (Desktop only) */}
                {!isMobile && (
                  <div>
                    <span style={{
                      padding: '0.25rem 0.75rem',
                      borderRadius: '20px',
                      fontSize: '0.75rem',
                      fontWeight: '600',
                      textTransform: 'uppercase',
                      backgroundColor: role.isActive ? '#d4edda' : '#f8d7da',
                      color: role.isActive ? '#155724' : '#721c24',
                      border: `1px solid ${role.isActive ? '#c3e6cb' : '#f5c6cb'}`
                    }}>
                      {role.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </div>
                )}

                {/* Type (Desktop only) */}
                {!isMobile && (
                  <div>
                    <span style={{
                      padding: '0.25rem 0.75rem',
                      borderRadius: '20px',
                      fontSize: '0.75rem',
                      fontWeight: '600',
                      textTransform: 'uppercase',
                      backgroundColor: role.isSystemRole ? '#e3f2fd' : '#fff3e0',
                      color: role.isSystemRole ? '#0d47a1' : '#e65100',
                      border: `1px solid ${role.isSystemRole ? '#bbdefb' : '#ffcc02'}`
                    }}>
                      {role.isSystemRole ? 'System' : 'Custom'}
                    </span>
                  </div>
                )}

                {/* Actions */}
                <div style={{
                  display: 'flex',
                  gap: '0.5rem',
                  alignItems: 'center'
                }}>
                  <button
                    onClick={() => openModal('view', role)}
                    style={{
                      width: '32px',
                      height: '32px',
                      border: 'none',
                      borderRadius: '6px',
                      backgroundColor: '#17a2b8',
                      color: 'white',
                      cursor: 'pointer',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '0.8rem'
                    }}
                    title="View Role"
                  >
                    <i className="fas fa-eye"></i>
                  </button>

                  <button
                    onClick={() => openModal('edit', role)}
                    style={{
                      width: '32px',
                      height: '32px',
                      border: 'none',
                      borderRadius: '6px',
                      backgroundColor: '#ffc107',
                      color: 'white',
                      cursor: 'pointer',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '0.8rem'
                    }}
                    title="Edit Role"
                  >
                    <i className="fas fa-edit"></i>
                  </button>

                  {!role.isSystemRole && (
                    <button
                      onClick={() => handleDelete(role)}
                      disabled={loading}
                      style={{
                        width: '32px',
                        height: '32px',
                        border: 'none',
                        borderRadius: '6px',
                        backgroundColor: '#dc3545',
                        color: 'white',
                        cursor: loading ? 'not-allowed' : 'pointer',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: '0.8rem',
                        opacity: loading ? 0.6 : 1
                      }}
                      title="Delete Role"
                    >
                      <i className="fas fa-trash"></i>
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Modal */}
      <Modal
        isOpen={isModalOpen}
        onRequestClose={closeModal}
        style={{
          overlay: {
            backgroundColor: 'rgba(0, 0, 0, 0.5)',
            zIndex: 1000
          },
          content: {
            top: '50%',
            left: '50%',
            right: 'auto',
            bottom: 'auto',
            marginRight: '-50%',
            transform: 'translate(-50%, -50%)',
            width: isMobile ? '95%' : '500px',
            maxHeight: '90vh',
            padding: '0',
            border: 'none',
            borderRadius: '12px',
            boxShadow: '0 20px 60px rgba(0, 0, 0, 0.2)'
          }
        }}
      >
        <div style={{
          background: 'linear-gradient(135deg, #20ABA0 0%, #2CE86F 100%)',
          padding: '1.5rem',
          borderRadius: '12px 12px 0 0',
          color: 'white'
        }}>
          <h3 style={{
            margin: '0',
            fontSize: '1.2rem',
            fontWeight: '600'
          }}>
            <i className={`fas fa-${modalMode === 'add' ? 'plus' : modalMode === 'edit' ? 'edit' : 'eye'}`} 
               style={{ marginRight: '0.5rem' }}></i>
            {modalMode === 'add' ? 'Add New Role' : modalMode === 'edit' ? 'Edit Role' : 'View Role'}
          </h3>
        </div>

        <form onSubmit={handleSubmit} style={{ padding: '2rem' }}>
          <div style={{ marginBottom: '1.5rem' }}>
            <label style={{
              display: 'block',
              marginBottom: '0.5rem',
              fontWeight: '600',
              color: '#2c3e50'
            }}>
              Role Name *
            </label>
            <input
              type="text"
              name="name"
              value={formData.name}
              onChange={handleInputChange}
              disabled={modalMode === 'view'}
              required
              style={{
                width: '100%',
                padding: '0.75rem',
                border: '1px solid #ddd',
                borderRadius: '8px',
                fontSize: '0.95rem',
                backgroundColor: modalMode === 'view' ? '#f8f9fa' : 'white'
              }}
              placeholder="Enter role name"
            />
          </div>

          <div style={{ marginBottom: '1.5rem' }}>
            <label style={{
              display: 'block',
              marginBottom: '0.5rem',
              fontWeight: '600',
              color: '#2c3e50'
            }}>
              System Name *
            </label>
            <input
              type="text"
              name="systemName"
              value={formData.systemName}
              onChange={handleInputChange}
              disabled={modalMode === 'view'}
              required
              style={{
                width: '100%',
                padding: '0.75rem',
                border: '1px solid #ddd',
                borderRadius: '8px',
                fontSize: '0.95rem',
                fontFamily: 'monospace',
                backgroundColor: modalMode === 'view' ? '#f8f9fa' : 'white'
              }}
              placeholder="SystemRoleName"
            />
          </div>

          <div style={{ marginBottom: '1.5rem' }}>
            <label style={{
              display: 'block',
              marginBottom: '0.5rem',
              fontWeight: '600',
              color: '#2c3e50'
            }}>
              Description
            </label>
            <textarea
              name="description"
              value={formData.description}
              onChange={handleInputChange}
              disabled={modalMode === 'view'}
              rows="3"
              style={{
                width: '100%',
                padding: '0.75rem',
                border: '1px solid #ddd',
                borderRadius: '8px',
                fontSize: '0.95rem',
                resize: 'vertical',
                backgroundColor: modalMode === 'view' ? '#f8f9fa' : 'white'
              }}
              placeholder="Enter role description"
            />
          </div>

          <div style={{
            display: 'flex',
            gap: '2rem',
            marginBottom: '2rem',
            flexDirection: isMobile ? 'column' : 'row'
          }}>
            <label style={{
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
              cursor: modalMode === 'view' ? 'default' : 'pointer'
            }}>
              <input
                type="checkbox"
                name="isActive"
                checked={formData.isActive}
                onChange={handleInputChange}
                disabled={modalMode === 'view'}
                style={{ accentColor: '#20ABA0' }}
              />
              <span style={{ fontWeight: '600', color: '#2c3e50' }}>Active</span>
            </label>

            <label style={{
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
              cursor: modalMode === 'view' ? 'default' : 'pointer'
            }}>
              <input
                type="checkbox"
                name="isSystemRole"
                checked={formData.isSystemRole}
                onChange={handleInputChange}
                disabled={modalMode === 'view'}
                style={{ accentColor: '#20ABA0' }}
              />
              <span style={{ fontWeight: '600', color: '#2c3e50' }}>System Role</span>
            </label>
          </div>

          <div style={{
            display: 'flex',
            gap: '1rem',
            justifyContent: 'flex-end',
            paddingTop: '1rem',
            borderTop: '1px solid #eee'
          }}>
            <button
              type="button"
              onClick={closeModal}
              style={{
                padding: '0.75rem 1.5rem',
                border: '1px solid #ddd',
                borderRadius: '8px',
                backgroundColor: 'white',
                color: '#6c757d',
                fontSize: '0.95rem',
                fontWeight: '600',
                cursor: 'pointer'
              }}
            >
              {modalMode === 'view' ? 'Close' : 'Cancel'}
            </button>

            {modalMode !== 'view' && (
              <button
                type="submit"
                disabled={loading}
                style={{
                  padding: '0.75rem 1.5rem',
                  border: 'none',
                  borderRadius: '8px',
                  background: 'linear-gradient(135deg, #20ABA0 0%, #2CE86F 100%)',
                  color: 'white',
                  fontSize: '0.95rem',
                  fontWeight: '600',
                  cursor: loading ? 'not-allowed' : 'pointer',
                  opacity: loading ? 0.7 : 1
                }}
              >
                {loading ? 'Saving...' : modalMode === 'add' ? 'Create Role' : 'Update Role'}
              </button>
            )}
          </div>
        </form>
      </Modal>

      <style jsx>{`
        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }
      `}</style>
    </div>
  );
};

export default RoleManagement;