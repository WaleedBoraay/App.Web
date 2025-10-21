import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import Modal from 'react-modal';
import { usersApi } from '../../services/apiService';

const UserManagement = ({ isMobile }) => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState('add'); // 'add', 'edit', 'view'
  const [selectedUser, setSelectedUser] = useState(null);
  const [formData, setFormData] = useState({
    userName: '',
    email: '',
    password: '',
    active: true,
    isLockedOut: false,
    departmentId: null,
    unitId: null,
    subUnitId: null
  });

  // Mock departments, units, subunits data - replace with actual API calls
  const [departments, setDepartments] = useState([]);
  const [units, setUnits] = useState([]);
  const [subUnits, setSubUnits] = useState([]);

  const columns = [
    { header: 'ID', accessor: 'id', sortable: true },
    { header: 'Username', accessor: 'userName', sortable: true },
    { header: 'Email', accessor: 'email', sortable: true },
    { 
      header: 'Status', 
      accessor: 'active',
      render: (value) => (
        <span style={{
          padding: '0.25rem 0.5rem',
          borderRadius: '12px',
          fontSize: '0.75rem',
          fontWeight: '500',
          background: value ? '#d4edda' : '#f8d7da',
          color: value ? '#155724' : '#721c24'
        }}>
          {value ? 'Active' : 'Inactive'}
        </span>
      )
    },
    { 
      header: 'Locked', 
      accessor: 'isLockedOut',
      render: (value) => value ? 'Yes' : 'No'
    },
    { header: 'Department', accessor: 'departmentName' },
    { header: 'Unit', accessor: 'unitName' },
    { header: 'Sub Unit', accessor: 'subUnitName' },
    { 
      header: 'Last Login', 
      accessor: 'lastLoginDateUtc',
      render: (value) => value ? new Date(value).toLocaleDateString() : 'Never'
    },
    { 
      header: 'Created', 
      accessor: 'createdOnUtc',
      render: (value) => new Date(value).toLocaleDateString()
    }
  ];

  const actions = [
    {
      label: 'View',
      icon: 'eye',
      onClick: (row) => handleViewUser(row),
      className: 'btn-info'
    },
    {
      label: 'Edit',
      icon: 'edit',
      onClick: (row) => handleEditUser(row),
      className: 'btn-warning'
    },
    {
      label: 'Delete',
      icon: 'trash',
      onClick: (row) => handleDeleteUser(row),
      className: 'btn-danger',
      confirm: true
    },
    {
      label: 'Toggle Lock',
      icon: 'lock',
      onClick: (row) => handleToggleLock(row),
      className: 'btn-secondary'
    }
  ];

  useEffect(() => {
    loadUsers();
    loadOrganizationData();
  }, []);

  const loadUsers = async () => {
    setLoading(true);
    try {
      const data = await usersApi.getAllUsers();
      setUsers(data);
    } catch (error) {
      console.error('Error loading users:', error);
      toast.error('Failed to load users');
      // Fallback to mock data for development
      setUsers([
        {
          id: 1,
          userName: 'admin',
          email: 'admin@example.com',
          active: true,
          isLockedOut: false,
          departmentName: 'IT Department',
          unitName: 'Development',
          subUnitName: 'Frontend',
          lastLoginDateUtc: '2025-10-07T10:30:00Z',
          createdOnUtc: '2025-01-15T08:00:00Z'
        },
        {
          id: 2,
          userName: 'manager1',
          email: 'manager@example.com',
          active: true,
          isLockedOut: false,
          departmentName: 'Operations',
          unitName: 'Customer Service',
          subUnitName: null,
          lastLoginDateUtc: '2025-10-06T15:45:00Z',
          createdOnUtc: '2025-02-20T09:15:00Z'
        }
      ]);
    } finally {
      setLoading(false);
    }
  };

  const loadOrganizationData = async () => {
    // Mock data - replace with actual API calls
    setDepartments([
      { id: 1, name: 'IT Department' },
      { id: 2, name: 'Operations' },
      { id: 3, name: 'Finance' }
    ]);
    
    setUnits([
      { id: 1, name: 'Development', departmentId: 1 },
      { id: 2, name: 'Customer Service', departmentId: 2 },
      { id: 3, name: 'Accounting', departmentId: 3 }
    ]);
    
    setSubUnits([
      { id: 1, name: 'Frontend', unitId: 1 },
      { id: 2, name: 'Backend', unitId: 1 },
      { id: 3, name: 'Phone Support', unitId: 2 }
    ]);
  };

  const handleAddUser = () => {
    setModalMode('add');
    setSelectedUser(null);
    setFormData({
      userName: '',
      email: '',
      password: '',
      active: true,
      isLockedOut: false,
      departmentId: null,
      unitId: null,
      subUnitId: null
    });
    setIsModalOpen(true);
  };

  const handleViewUser = async (user) => {
    try {
      const userDetails = await usersApi.getUserById(user.id);
      setSelectedUser(userDetails);
    } catch (error) {
      console.error('Error fetching user details:', error);
      setSelectedUser(user);
    }
    
    setModalMode('view');
    setIsModalOpen(true);
  };

  const handleEditUser = (user) => {
    setModalMode('edit');
    setSelectedUser(user);
    setFormData({
      userName: user.userName,
      email: user.email,
      password: '',
      active: user.active,
      isLockedOut: user.isLockedOut,
      departmentId: user.departmentId,
      unitId: user.unitId,
      subUnitId: user.subUnitId
    });
    setIsModalOpen(true);
  };

  const handleDeleteUser = async (user) => {
    if (!window.confirm(`Are you sure you want to delete user "${user.userName}"?`)) {
      return;
    }

    try {
      await usersApi.deleteUser(user.id);
      setUsers(prev => prev.filter(u => u.id !== user.id));
      toast.success('User deleted successfully');
    } catch (error) {
      console.error('Error deleting user:', error);
      toast.error('Failed to delete user');
    }
  };

  const handleToggleLock = async (user) => {
    try {
      if (user.isLockedOut) {
        await usersApi.unlockUser(user.id);
      } else {
        await usersApi.lockUser(user.id);
      }

      setUsers(prev => prev.map(u => 
        u.id === user.id 
          ? { ...u, isLockedOut: !u.isLockedOut }
          : u
      ));
      toast.success(`User ${user.isLockedOut ? 'unlocked' : 'locked'} successfully`);
    } catch (error) {
      console.error('Error toggling user lock:', error);
      toast.error('Failed to update user lock status');
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    try {
      if (modalMode === 'add') {
        await usersApi.createUser(formData);
      } else {
        await usersApi.updateUser(selectedUser.id, formData);
      }

      await loadUsers();
      setIsModalOpen(false);
      toast.success(`User ${modalMode === 'add' ? 'created' : 'updated'} successfully`);
    } catch (error) {
      console.error(`Error ${modalMode}ing user:`, error);
      toast.error(`Failed to ${modalMode} user`);
    }
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setSelectedUser(null);
    setFormData({
      userName: '',
      email: '',
      password: '',
      active: true,
      isLockedOut: false,
      departmentId: null,
      unitId: null,
      subUnitId: null
    });
  };

  const renderUserForm = () => (
    <form onSubmit={handleSubmit}>
      <div style={{ display: 'grid', gap: '1rem', gridTemplateColumns: '1fr 1fr' }}>
        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
            Username *
          </label>
          <input
            type="text"
            value={formData.userName}
            onChange={(e) => setFormData(prev => ({ ...prev, userName: e.target.value }))}
            required
            style={{
              width: '100%',
              padding: '0.75rem',
              border: '1px solid #ddd',
              borderRadius: '6px',
              fontSize: '0.9rem'
            }}
          />
        </div>
        
        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
            Email *
          </label>
          <input
            type="email"
            value={formData.email}
            onChange={(e) => setFormData(prev => ({ ...prev, email: e.target.value }))}
            required
            style={{
              width: '100%',
              padding: '0.75rem',
              border: '1px solid #ddd',
              borderRadius: '6px',
              fontSize: '0.9rem'
            }}
          />
        </div>

        <div style={{ gridColumn: '1 / -1' }}>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
            Password {modalMode === 'add' ? '*' : '(Leave empty to keep current)'}
          </label>
          <input
            type="password"
            value={formData.password}
            onChange={(e) => setFormData(prev => ({ ...prev, password: e.target.value }))}
            required={modalMode === 'add'}
            style={{
              width: '100%',
              padding: '0.75rem',
              border: '1px solid #ddd',
              borderRadius: '6px',
              fontSize: '0.9rem'
            }}
          />
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
            Department
          </label>
          <select
            value={formData.departmentId || ''}
            onChange={(e) => setFormData(prev => ({ ...prev, departmentId: e.target.value || null }))}
            style={{
              width: '100%',
              padding: '0.75rem',
              border: '1px solid #ddd',
              borderRadius: '6px',
              fontSize: '0.9rem'
            }}
          >
            <option value="">Select Department</option>
            {departments.map(dept => (
              <option key={dept.id} value={dept.id}>{dept.name}</option>
            ))}
          </select>
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
            Unit
          </label>
          <select
            value={formData.unitId || ''}
            onChange={(e) => setFormData(prev => ({ ...prev, unitId: e.target.value || null }))}
            style={{
              width: '100%',
              padding: '0.75rem',
              border: '1px solid #ddd',
              borderRadius: '6px',
              fontSize: '0.9rem'
            }}
          >
            <option value="">Select Unit</option>
            {units.filter(unit => !formData.departmentId || unit.departmentId === parseInt(formData.departmentId)).map(unit => (
              <option key={unit.id} value={unit.id}>{unit.name}</option>
            ))}
          </select>
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
            Sub Unit
          </label>
          <select
            value={formData.subUnitId || ''}
            onChange={(e) => setFormData(prev => ({ ...prev, subUnitId: e.target.value || null }))}
            style={{
              width: '100%',
              padding: '0.75rem',
              border: '1px solid #ddd',
              borderRadius: '6px',
              fontSize: '0.9rem'
            }}
          >
            <option value="">Select Sub Unit</option>
            {subUnits.filter(subUnit => !formData.unitId || subUnit.unitId === parseInt(formData.unitId)).map(subUnit => (
              <option key={subUnit.id} value={subUnit.id}>{subUnit.name}</option>
            ))}
          </select>
        </div>

        <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <input
              type="checkbox"
              checked={formData.active}
              onChange={(e) => setFormData(prev => ({ ...prev, active: e.target.checked }))}
            />
            Active
          </label>
          
          <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <input
              type="checkbox"
              checked={formData.isLockedOut}
              onChange={(e) => setFormData(prev => ({ ...prev, isLockedOut: e.target.checked }))}
            />
            Locked Out
          </label>
        </div>
      </div>

      <div style={{ marginTop: '2rem', textAlign: 'right', display: 'flex', gap: '1rem', justifyContent: 'flex-end' }}>
        <button
          type="button"
          onClick={closeModal}
          style={{
            padding: '0.75rem 1.5rem',
            border: '1px solid #ddd',
            borderRadius: '6px',
            background: 'white',
            cursor: 'pointer'
          }}
        >
          Cancel
        </button>
        <button
          type="submit"
          style={{
            padding: '0.75rem 1.5rem',
            border: 'none',
            borderRadius: '6px',
            background: 'linear-gradient(135deg, #243483 0%, #1467EA 100%)',
            color: 'white',
            cursor: 'pointer'
          }}
        >
          {modalMode === 'add' ? 'Create User' : 'Update User'}
        </button>
      </div>
    </form>
  );

  const renderUserDetails = () => (
    <div style={{ display: 'grid', gap: '1rem' }}>
      <div><strong>ID:</strong> {selectedUser.id}</div>
      <div><strong>Username:</strong> {selectedUser.userName || selectedUser.username}</div>
      <div><strong>Email:</strong> {selectedUser.email}</div>
      <div><strong>Status:</strong> {selectedUser.active || selectedUser.isActive ? 'Active' : 'Inactive'}</div>
      <div><strong>Locked:</strong> {selectedUser.isLockedOut ? 'Yes' : 'No'}</div>
      <div><strong>Department:</strong> {selectedUser.departmentName || 'N/A'}</div>
      <div><strong>Unit:</strong> {selectedUser.unitName || 'N/A'}</div>
      <div><strong>Sub Unit:</strong> {selectedUser.subUnitName || 'N/A'}</div>
      {selectedUser.roles && (
        <div><strong>Roles:</strong> {selectedUser.roles.map(r => r.name || r.Name).join(', ')}</div>
      )}
      <div><strong>Last Login:</strong> {selectedUser.lastLoginDateUtc ? new Date(selectedUser.lastLoginDateUtc).toLocaleString() : 'Never'}</div>
      <div><strong>Created:</strong> {new Date(selectedUser.createdOnUtc).toLocaleString()}</div>
    </div>
  );

  const modalStyles = {
    content: {
      top: '50%',
      left: '50%',
      right: 'auto',
      bottom: 'auto',
      marginRight: '-50%',
      transform: 'translate(-50%, -50%)',
      width: isMobile ? '95%' : '70%',
      maxWidth: '800px',
      borderRadius: '12px',
      boxShadow: '0 10px 40px rgba(0,0,0,0.3)',
      padding: '2rem',
      border: 'none',
      maxHeight: '90vh',
      overflowY: 'auto'
    },
    overlay: {
      backgroundColor: 'rgba(0, 0, 0, 0.5)',
      zIndex: 1000
    }
  };

  return (
    <div>
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center', 
        marginBottom: '1.5rem',
        flexWrap: 'wrap',
        gap: '1rem'
      }}>
        <h2 style={{ margin: 0, color: '#243483' }}>User Management</h2>
        <button
          onClick={handleAddUser}
          style={{
            background: 'linear-gradient(135deg, #243483 0%, #1467EA 100%)',
            color: 'white',
            border: 'none',
            padding: '0.75rem 1.5rem',
            borderRadius: '8px',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            gap: '0.5rem',
            fontWeight: '500'
          }}
        >
          <i className="fas fa-plus"></i>
          Add User
        </button>
      </div>

      <div style={{
        background: 'white',
        borderRadius: '12px',
        boxShadow: '0 2px 10px rgba(0,0,0,0.05)',
        overflow: 'hidden'
      }}>
        {loading ? (
          <div style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '300px'
          }}>
            <div style={{ textAlign: 'center' }}>
              <i className="fas fa-spinner fa-spin" style={{ fontSize: '2rem', color: '#243483', marginBottom: '1rem' }}></i>
              <p>Loading users...</p>
            </div>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr style={{ backgroundColor: '#f1f3f9', borderBottom: '1px solid #e9ecef' }}>
                  {columns.map((column, index) => (
                    <th key={index} style={{
                      padding: '1rem 1.25rem',
                      textAlign: 'left',
                      fontWeight: 600,
                      color: '#495057',
                      fontSize: '0.9rem',
                      textTransform: 'uppercase',
                      letterSpacing: '0.5px'
                    }}>
                      {column.header}
                    </th>
                  ))}
                  <th style={{ width: '200px', textAlign: 'center' }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {users.length > 0 ? (
                  users.map((user, rowIndex) => (
                    <tr key={rowIndex} style={{ borderBottom: '1px solid #e9ecef' }}>
                      {columns.map((column, colIndex) => (
                        <td key={colIndex} style={{
                          padding: '1rem 1.25rem',
                          color: '#495057',
                          fontSize: '0.95rem'
                        }}>
                          {column.render ? column.render(user[column.accessor], user) : (user[column.accessor] || '-')}
                        </td>
                      ))}
                      <td style={{ padding: '1rem', textAlign: 'center' }}>
                        {actions.map((action, actionIndex) => (
                          <button 
                            key={actionIndex}
                            onClick={() => {
                              if (action.confirm) {
                                const confirmMessage = `Are you sure you want to ${action.label ? action.label.toLowerCase() : 'perform this action'}?`;
                                if (window.confirm(confirmMessage)) {
                                  action.onClick(user);
                                }
                              } else {
                                action.onClick(user);
                              }
                            }}
                            style={{
                              background: 'transparent',
                              border: `1px solid ${
                                action.className === 'btn-danger' ? '#dc3545' : 
                                action.className === 'btn-warning' ? '#ffc107' :
                                action.className === 'btn-info' ? '#17a2b8' :
                                action.className === 'btn-secondary' ? '#6c757d' :
                                '#243483'
                              }`,
                              color: action.className === 'btn-danger' ? '#dc3545' : 
                                    action.className === 'btn-warning' ? '#ffc107' :
                                    action.className === 'btn-info' ? '#17a2b8' :
                                    action.className === 'btn-secondary' ? '#6c757d' :
                                    '#243483',
                              cursor: 'pointer',
                              margin: '0 0.25rem',
                              padding: '0.4rem 0.6rem',
                              borderRadius: '4px',
                              transition: 'all 0.2s ease',
                              fontSize: '0.8rem'
                            }}
                            title={action.label}
                          >
                            <i className={`fas fa-${action.icon}`}></i>
                          </button>
                        ))}
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td 
                      colSpan={columns.length + 1}
                      style={{
                        padding: '2rem',
                        textAlign: 'center',
                        color: '#6c757d',
                        fontStyle: 'italic'
                      }}
                    >
                      No users found
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <Modal
        isOpen={isModalOpen}
        onRequestClose={closeModal}
        style={modalStyles}
        contentLabel={`${modalMode} User`}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
          <h3 style={{ margin: 0, color: '#243483' }}>
            {modalMode === 'add' ? 'Add New User' : 
             modalMode === 'edit' ? 'Edit User' : 'User Details'}
          </h3>
          <button
            onClick={closeModal}
            style={{
              background: 'transparent',
              border: 'none',
              fontSize: '1.5rem',
              cursor: 'pointer',
              color: '#6c757d'
            }}
          >
            <i className="fas fa-times"></i>
          </button>
        </div>
        
        {modalMode === 'view' ? renderUserDetails() : renderUserForm()}
      </Modal>
    </div>
  );
};

export default UserManagement;