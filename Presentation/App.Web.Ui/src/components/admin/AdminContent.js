import React from 'react';
import UserManagement from './UserManagement';
import UserRoles from './UserRoles';
import UserPermissionOverrides from './UserPermissionOverrides';
import RoleManagement from './RoleManagement';
import RolePermissionMapping from './RolePermissionMapping';
import PermissionManagement from './PermissionManagement';
import LanguageManagement from './LanguageManagement';
import ResourcesManagement from './ResourcesManagement';
import EntityProfile from './EntityProfile';

const AdminContent = ({ 
  activeTab, 
  tableColumns, 
  tableData, 
  isLoading, 
  onAddNew, 
  onEdit, 
  onDelete,
  isMobile 
}) => {
  const renderTabContent = () => {
    switch (activeTab) {
      case 'users':
        return (
          <UserManagement isMobile={isMobile} />
        );
      case 'user-roles':
        return (
          <UserRoles isMobile={isMobile} />
        );
      case 'user-permissions':
        return (
          <UserPermissionOverrides isMobile={isMobile} />
        );
      case 'role-permissions':
        return (
          <RolePermissionMapping isMobile={isMobile} />
        );
      case 'permissions':
        return (
          <PermissionManagement isMobile={isMobile} />
        );
      case 'languages':
        return (
          <LanguageManagement isMobile={isMobile} />
        );
      case 'resources':
        return (
          <ResourcesManagement isMobile={isMobile} />
        );
      case 'entity-profile':
        return (
          <EntityProfile isMobile={isMobile} />
        );
      case 'roles':
        return (
          <RoleManagement isMobile={isMobile} />
        );
      case 'institutions':
        return (
          <div style={{ textAlign: 'center', padding: '2rem', color: '#6c757d' }}>
            <i className="fas fa-building" style={{ fontSize: '3rem', marginBottom: '1rem' }}></i>
            <h3>Institution Management</h3>
            <p>Institution management features coming soon...</p>
          </div>
        );
      case 'settings':
        return (
          <div style={{ textAlign: 'center', padding: '2rem', color: '#6c757d' }}>
            <i className="fas fa-cog" style={{ fontSize: '3rem', marginBottom: '1rem' }}></i>
            <h3>System Settings</h3>
            <p>System settings features coming soon...</p>
          </div>
        );
      case 'reports':
        return (
          <div style={{ textAlign: 'center', padding: '2rem', color: '#6c757d' }}>
            <i className="fas fa-chart-bar" style={{ fontSize: '3rem', marginBottom: '1rem' }}></i>
            <h3>Reports</h3>
            <p>Reporting features coming soon...</p>
          </div>
        );
      default:
        return (
          <div style={{
            textAlign: 'center',
            padding: '2rem',
            color: '#6c757d'
          }}>
            <i className="fas fa-info-circle" style={{ fontSize: '3rem', marginBottom: '1rem' }}></i>
            <h3>Select a tab to view content</h3>
            <p>Choose from the sidebar to manage different aspects of the system.</p>
          </div>
        );
    }
  };

  return (
    <div style={{ 
      background: 'white', 
      borderRadius: '12px', 
      boxShadow: '0 4px 20px rgba(0,0,0,0.08)', 
      padding: isMobile ? '1rem' : '1.5rem', 
      minHeight: '500px',
      border: '1px solid #f0f0f0'
    }}>
      {renderTabContent()}
    </div>
  );
};

export default AdminContent;
