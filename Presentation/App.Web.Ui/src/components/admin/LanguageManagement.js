import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import Modal from 'react-modal';
import { languagesApi } from '../../services/apiService';

// Set app element for Modal accessibility
if (typeof window !== 'undefined') {
  Modal.setAppElement(document.getElementById('root') || document.body);
}

const LanguageManagement = () => {
  const [languages, setLanguages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingLanguage, setEditingLanguage] = useState(null);
  const [formData, setFormData] = useState({
    name: '',
    languageCulture: '',
    uniqueSeoCode: '',
    flagImageFileName: '',
    rtl: false,
    published: true,
    displayOrder: 1
  });
  const [filterText, setFilterText] = useState('');
  const [sortField, setSortField] = useState('displayOrder');
  const [sortDirection, setSortDirection] = useState('asc');
  const [showActiveOnly, setShowActiveOnly] = useState(false);

  useEffect(() => {
    loadLanguages();
  }, []);

  const loadLanguages = async () => {
    setLoading(true);
    try {
      const data = await languagesApi.getAll();
      setLanguages(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error('Error loading languages:', error);
      toast.error('Failed to load languages');
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setEditingLanguage(null);
    setFormData({
      name: '',
      languageCulture: '',
      uniqueSeoCode: '',
      flagImageFileName: '',
      rtl: false,
      published: true,
      displayOrder: Math.max(...languages.map(l => l.displayOrder || 0), 0) + 1
    });
    setModalOpen(true);
  };

  const handleEdit = (language) => {
    setEditingLanguage(language);
    setFormData({
      name: language.name || '',
      languageCulture: language.languageCulture || '',
      uniqueSeoCode: language.uniqueSeoCode || '',
      flagImageFileName: language.flagImageFileName || '',
      rtl: language.rtl || false,
      published: language.published !== undefined ? language.published : true,
      displayOrder: language.displayOrder || 1
    });
    setModalOpen(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      if (editingLanguage) {
        await languagesApi.update(editingLanguage.id, formData);
        toast.success('Language updated successfully');
      } else {
        await languagesApi.create(formData);
        toast.success('Language created successfully');
      }
      
      setModalOpen(false);
      loadLanguages();
    } catch (error) {
      console.error('Error saving language:', error);
      toast.error(`Failed to ${editingLanguage ? 'update' : 'create'} language`);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (language) => {
    if (!window.confirm(`Are you sure you want to delete "${language.name}"?`)) {
      return;
    }

    setLoading(true);
    try {
      await languagesApi.delete(language.id);
      toast.success('Language deleted successfully');
      loadLanguages();
    } catch (error) {
      console.error('Error deleting language:', error);
      toast.error('Failed to delete language');
    } finally {
      setLoading(false);
    }
  };

  const handleChangeLanguage = async (language) => {
    try {
      await languagesApi.changeLanguage(language.uniqueSeoCode, window.location.href);
      toast.success(`Language changed to ${language.name}`);
      // Optionally reload the page or update the UI language
      window.location.reload();
    } catch (error) {
      console.error('Error changing language:', error);
      toast.error('Failed to change language');
    }
  };

  const handleSort = (field) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDirection('asc');
    }
  };

  const getSortIcon = (field) => {
    if (sortField !== field) return '↕️';
    return sortDirection === 'asc' ? '↑' : '↓';
  };

  const filteredLanguages = languages
    .filter(language => {
      const matchesText = !filterText || 
        language.name?.toLowerCase().includes(filterText.toLowerCase()) ||
        language.languageCulture?.toLowerCase().includes(filterText.toLowerCase()) ||
        language.uniqueSeoCode?.toLowerCase().includes(filterText.toLowerCase());
      
      const matchesStatus = !showActiveOnly || language.published;
      
      return matchesText && matchesStatus;
    })
    .sort((a, b) => {
      const aVal = a[sortField] || '';
      const bVal = b[sortField] || '';
      
      if (typeof aVal === 'string') {
        return sortDirection === 'asc' 
          ? aVal.localeCompare(bVal)
          : bVal.localeCompare(aVal);
      }
      
      return sortDirection === 'asc' 
        ? (aVal < bVal ? -1 : aVal > bVal ? 1 : 0)
        : (bVal < aVal ? -1 : bVal > aVal ? 1 : 0);
    });

  const modalStyles = {
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
      padding: '0',
      border: 'none',
      borderRadius: '8px',
      maxWidth: '600px',
      width: '90%',
      maxHeight: '90vh',
      overflow: 'auto'
    }
  };

  return (
    <div style={{ padding: '24px' }}>
      {/* Header */}
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center', 
        marginBottom: '24px',
        flexWrap: 'wrap',
        gap: '12px'
      }}>
        <div>
          <h2 style={{ margin: '0 0 8px 0', fontSize: '24px', fontWeight: '600' }}>
            Language Management
          </h2>
          <p style={{ margin: '0', color: '#666', fontSize: '14px' }}>
            Manage system languages and localization settings
          </p>
        </div>
        <button
          onClick={handleCreate}
          disabled={loading}
          style={{
            backgroundColor: '#007bff',
            color: 'white',
            border: 'none',
            padding: '10px 20px',
            borderRadius: '6px',
            cursor: loading ? 'not-allowed' : 'pointer',
            fontSize: '14px',
            fontWeight: '500',
            opacity: loading ? 0.6 : 1,
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}
        >
          <span>➕</span>
          Add Language
        </button>
      </div>

      {/* Statistics Cards */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
        gap: '16px',
        marginBottom: '24px'
      }}>
        <div style={{
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          color: 'white',
          padding: '20px',
          borderRadius: '8px',
          textAlign: 'center'
        }}>
          <h3 style={{ margin: '0 0 8px 0', fontSize: '24px', fontWeight: '700' }}>
            {languages.length}
          </h3>
          <p style={{ margin: 0, fontSize: '14px', opacity: 0.9 }}>Total Languages</p>
        </div>
        <div style={{
          background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
          color: 'white',
          padding: '20px',
          borderRadius: '8px',
          textAlign: 'center'
        }}>
          <h3 style={{ margin: '0 0 8px 0', fontSize: '24px', fontWeight: '700' }}>
            {languages.filter(l => l.published).length}
          </h3>
          <p style={{ margin: 0, fontSize: '14px', opacity: 0.9 }}>Published Languages</p>
        </div>
        <div style={{
          background: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
          color: 'white',
          padding: '20px',
          borderRadius: '8px',
          textAlign: 'center'
        }}>
          <h3 style={{ margin: '0 0 8px 0', fontSize: '24px', fontWeight: '700' }}>
            {languages.filter(l => l.rtl).length}
          </h3>
          <p style={{ margin: 0, fontSize: '14px', opacity: 0.9 }}>RTL Languages</p>
        </div>
      </div>

      {/* Filters */}
      <div style={{ 
        display: 'flex', 
        gap: '16px', 
        marginBottom: '20px',
        flexWrap: 'wrap',
        alignItems: 'center'
      }}>
        <input
          type="text"
          placeholder="Search languages..."
          value={filterText}
          onChange={(e) => setFilterText(e.target.value)}
          style={{
            padding: '8px 12px',
            border: '1px solid #ddd',
            borderRadius: '4px',
            fontSize: '14px',
            minWidth: '200px'
          }}
        />
        <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '14px' }}>
          <input
            type="checkbox"
            checked={showActiveOnly}
            onChange={(e) => setShowActiveOnly(e.target.checked)}
          />
          Published only
        </label>
      </div>

      {/* Languages Table */}
      <div style={{ 
        backgroundColor: 'white', 
        border: '1px solid #e0e0e0', 
        borderRadius: '8px',
        overflow: 'hidden',
        boxShadow: '0 2px 4px rgba(0,0,0,0.1)'
      }}>
        {loading ? (
          <div style={{ 
            padding: '40px', 
            textAlign: 'center', 
            color: '#666',
            fontSize: '16px'
          }}>
            Loading languages...
          </div>
        ) : filteredLanguages.length === 0 ? (
          <div style={{ 
            padding: '40px', 
            textAlign: 'center', 
            color: '#666',
            fontSize: '16px'
          }}>
            No languages found
          </div>
        ) : (
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ backgroundColor: '#f8f9fa' }}>
                <th 
                  onClick={() => handleSort('displayOrder')}
                  style={{ 
                    padding: '12px', 
                    textAlign: 'left', 
                    borderBottom: '1px solid #e0e0e0',
                    cursor: 'pointer',
                    userSelect: 'none',
                    fontSize: '14px',
                    fontWeight: '600'
                  }}
                >
                  Order {getSortIcon('displayOrder')}
                </th>
                <th 
                  onClick={() => handleSort('name')}
                  style={{ 
                    padding: '12px', 
                    textAlign: 'left', 
                    borderBottom: '1px solid #e0e0e0',
                    cursor: 'pointer',
                    userSelect: 'none',
                    fontSize: '14px',
                    fontWeight: '600'
                  }}
                >
                  Language {getSortIcon('name')}
                </th>
                <th 
                  onClick={() => handleSort('languageCulture')}
                  style={{ 
                    padding: '12px', 
                    textAlign: 'left', 
                    borderBottom: '1px solid #e0e0e0',
                    cursor: 'pointer',
                    userSelect: 'none',
                    fontSize: '14px',
                    fontWeight: '600'
                  }}
                >
                  Culture {getSortIcon('languageCulture')}
                </th>
                <th 
                  onClick={() => handleSort('uniqueSeoCode')}
                  style={{ 
                    padding: '12px', 
                    textAlign: 'left', 
                    borderBottom: '1px solid #e0e0e0',
                    cursor: 'pointer',
                    userSelect: 'none',
                    fontSize: '14px',
                    fontWeight: '600'
                  }}
                >
                  SEO Code {getSortIcon('uniqueSeoCode')}
                </th>
                <th style={{ 
                  padding: '12px', 
                  textAlign: 'center', 
                  borderBottom: '1px solid #e0e0e0',
                  fontSize: '14px',
                  fontWeight: '600'
                }}>
                  RTL
                </th>
                <th style={{ 
                  padding: '12px', 
                  textAlign: 'center', 
                  borderBottom: '1px solid #e0e0e0',
                  fontSize: '14px',
                  fontWeight: '600'
                }}>
                  Published
                </th>
                <th style={{ 
                  padding: '12px', 
                  textAlign: 'center', 
                  borderBottom: '1px solid #e0e0e0',
                  fontSize: '14px',
                  fontWeight: '600'
                }}>
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {filteredLanguages.map((language, index) => (
                <tr 
                  key={language.id}
                  style={{ 
                    backgroundColor: index % 2 === 0 ? '#ffffff' : '#f8f9fa',
                    borderBottom: '1px solid #e0e0e0'
                  }}
                >
                  <td style={{ 
                    padding: '12px', 
                    fontSize: '14px',
                    fontWeight: '500'
                  }}>
                    {language.displayOrder}
                  </td>
                  <td style={{ padding: '12px', fontSize: '14px' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                      {language.flagImageFileName && (
                        <span style={{ 
                          fontSize: '18px',
                          width: '24px',
                          height: '16px',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center'
                        }}>
                          🏳️
                        </span>
                      )}
                      <span style={{ fontWeight: '500' }}>{language.name}</span>
                    </div>
                  </td>
                  <td style={{ 
                    padding: '12px', 
                    fontSize: '14px',
                    fontFamily: 'monospace',
                    color: '#666'
                  }}>
                    {language.languageCulture}
                  </td>
                  <td style={{ 
                    padding: '12px', 
                    fontSize: '14px',
                    fontFamily: 'monospace',
                    fontWeight: '500'
                  }}>
                    {language.uniqueSeoCode}
                  </td>
                  <td style={{ padding: '12px', textAlign: 'center' }}>
                    <span style={{ 
                      fontSize: '16px',
                      color: language.rtl ? '#28a745' : '#6c757d'
                    }}>
                      {language.rtl ? '✓' : '✗'}
                    </span>
                  </td>
                  <td style={{ padding: '12px', textAlign: 'center' }}>
                    <span
                      style={{
                        padding: '4px 8px',
                        borderRadius: '12px',
                        fontSize: '12px',
                        fontWeight: '500',
                        backgroundColor: language.published ? '#d4edda' : '#f8d7da',
                        color: language.published ? '#155724' : '#721c24'
                      }}
                    >
                      {language.published ? 'Published' : 'Draft'}
                    </span>
                  </td>
                  <td style={{ padding: '12px' }}>
                    <div style={{ 
                      display: 'flex', 
                      gap: '8px', 
                      justifyContent: 'center',
                      flexWrap: 'wrap'
                    }}>
                      <button
                        onClick={() => handleChangeLanguage(language)}
                        style={{
                          backgroundColor: '#28a745',
                          color: 'white',
                          border: 'none',
                          padding: '6px 12px',
                          borderRadius: '4px',
                          cursor: 'pointer',
                          fontSize: '12px',
                          fontWeight: '500'
                        }}
                        title="Switch to this language"
                      >
                        🌐 Use
                      </button>
                      <button
                        onClick={() => handleEdit(language)}
                        style={{
                          backgroundColor: '#ffc107',
                          color: '#000',
                          border: 'none',
                          padding: '6px 12px',
                          borderRadius: '4px',
                          cursor: 'pointer',
                          fontSize: '12px',
                          fontWeight: '500'
                        }}
                      >
                        ✏️ Edit
                      </button>
                      <button
                        onClick={() => handleDelete(language)}
                        style={{
                          backgroundColor: '#dc3545',
                          color: 'white',
                          border: 'none',
                          padding: '6px 12px',
                          borderRadius: '4px',
                          cursor: 'pointer',
                          fontSize: '12px',
                          fontWeight: '500'
                        }}
                      >
                        🗑️ Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Language Form Modal */}
      <Modal
        isOpen={modalOpen}
        onRequestClose={() => setModalOpen(false)}
        style={modalStyles}
        contentLabel={editingLanguage ? "Edit Language" : "Create Language"}
      >
        <div style={{ 
          padding: '24px',
          backgroundColor: 'white'
        }}>
          <div style={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center', 
            marginBottom: '20px',
            borderBottom: '1px solid #e0e0e0',
            paddingBottom: '16px'
          }}>
            <h3 style={{ margin: 0, fontSize: '20px', fontWeight: '600' }}>
              {editingLanguage ? 'Edit Language' : 'Create New Language'}
            </h3>
            <button
              onClick={() => setModalOpen(false)}
              style={{
                background: 'none',
                border: 'none',
                fontSize: '24px',
                cursor: 'pointer',
                color: '#666',
                padding: '0',
                width: '30px',
                height: '30px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center'
              }}
            >
              ×
            </button>
          </div>

          <form onSubmit={handleSubmit}>
            <div style={{ display: 'grid', gap: '16px' }}>
              <div>
                <label style={{ 
                  display: 'block', 
                  marginBottom: '4px', 
                  fontSize: '14px', 
                  fontWeight: '500' 
                }}>
                  Language Name *
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  required
                  style={{
                    width: '100%',
                    padding: '8px 12px',
                    border: '1px solid #ddd',
                    borderRadius: '4px',
                    fontSize: '14px'
                  }}
                  placeholder="e.g., English, العربية, Français"
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                <div>
                  <label style={{ 
                    display: 'block', 
                    marginBottom: '4px', 
                    fontSize: '14px', 
                    fontWeight: '500' 
                  }}>
                    Language Culture *
                  </label>
                  <input
                    type="text"
                    value={formData.languageCulture}
                    onChange={(e) => setFormData({ ...formData, languageCulture: e.target.value })}
                    required
                    style={{
                      width: '100%',
                      padding: '8px 12px',
                      border: '1px solid #ddd',
                      borderRadius: '4px',
                      fontSize: '14px',
                      fontFamily: 'monospace'
                    }}
                    placeholder="e.g., en-US, ar-SA, fr-FR"
                  />
                </div>

                <div>
                  <label style={{ 
                    display: 'block', 
                    marginBottom: '4px', 
                    fontSize: '14px', 
                    fontWeight: '500' 
                  }}>
                    SEO Code *
                  </label>
                  <input
                    type="text"
                    value={formData.uniqueSeoCode}
                    onChange={(e) => setFormData({ ...formData, uniqueSeoCode: e.target.value })}
                    required
                    style={{
                      width: '100%',
                      padding: '8px 12px',
                      border: '1px solid #ddd',
                      borderRadius: '4px',
                      fontSize: '14px',
                      fontFamily: 'monospace'
                    }}
                    placeholder="e.g., en, ar, fr"
                  />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                <div>
                  <label style={{ 
                    display: 'block', 
                    marginBottom: '4px', 
                    fontSize: '14px', 
                    fontWeight: '500' 
                  }}>
                    Flag Image File
                  </label>
                  <input
                    type="text"
                    value={formData.flagImageFileName}
                    onChange={(e) => setFormData({ ...formData, flagImageFileName: e.target.value })}
                    style={{
                      width: '100%',
                      padding: '8px 12px',
                      border: '1px solid #ddd',
                      borderRadius: '4px',
                      fontSize: '14px'
                    }}
                    placeholder="e.g., us.png, sa.png"
                  />
                </div>

                <div>
                  <label style={{ 
                    display: 'block', 
                    marginBottom: '4px', 
                    fontSize: '14px', 
                    fontWeight: '500' 
                  }}>
                    Display Order
                  </label>
                  <input
                    type="number"
                    value={formData.displayOrder}
                    onChange={(e) => setFormData({ ...formData, displayOrder: parseInt(e.target.value) || 1 })}
                    min="1"
                    style={{
                      width: '100%',
                      padding: '8px 12px',
                      border: '1px solid #ddd',
                      borderRadius: '4px',
                      fontSize: '14px'
                    }}
                  />
                </div>
              </div>

              <div style={{ display: 'flex', gap: '20px' }}>
                <label style={{ 
                  display: 'flex', 
                  alignItems: 'center', 
                  gap: '8px', 
                  fontSize: '14px',
                  cursor: 'pointer'
                }}>
                  <input
                    type="checkbox"
                    checked={formData.rtl}
                    onChange={(e) => setFormData({ ...formData, rtl: e.target.checked })}
                  />
                  Right-to-Left (RTL)
                </label>

                <label style={{ 
                  display: 'flex', 
                  alignItems: 'center', 
                  gap: '8px', 
                  fontSize: '14px',
                  cursor: 'pointer'
                }}>
                  <input
                    type="checkbox"
                    checked={formData.published}
                    onChange={(e) => setFormData({ ...formData, published: e.target.checked })}
                  />
                  Published
                </label>
              </div>
            </div>

            <div style={{ 
              display: 'flex', 
              gap: '12px', 
              justifyContent: 'flex-end',
              marginTop: '24px',
              paddingTop: '16px',
              borderTop: '1px solid #e0e0e0'
            }}>
              <button
                type="button"
                onClick={() => setModalOpen(false)}
                style={{
                  backgroundColor: '#6c757d',
                  color: 'white',
                  border: 'none',
                  padding: '10px 20px',
                  borderRadius: '6px',
                  cursor: 'pointer',
                  fontSize: '14px'
                }}
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={loading}
                style={{
                  backgroundColor: '#007bff',
                  color: 'white',
                  border: 'none',
                  padding: '10px 20px',
                  borderRadius: '6px',
                  cursor: loading ? 'not-allowed' : 'pointer',
                  fontSize: '14px',
                  opacity: loading ? 0.6 : 1
                }}
              >
                {loading ? 'Saving...' : (editingLanguage ? 'Update Language' : 'Create Language')}
              </button>
            </div>
          </form>
        </div>
      </Modal>
    </div>
  );
};

export default LanguageManagement;