using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HoardersDAL;

namespace Hoarders.Maintenance
{
    public partial class AddTaxDetails : GetData
    {
        private Security.PrivilegeAction m_PageMode = Security.PrivilegeAction.None;
        private string m_ID = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                if (!Page.IsPostBack)
                {
                    CheckSession();
                    m_ID = HoardersDAL.Globals.QueryString_GetKey("ID", "-1");
                    if (HoardersDAL.Globals.SecurityClass.HasPermission(((int)Security.Menu.TaxDetails).ToString(), Security.PrivilegeAction.Full))
                        m_PageMode = Security.PrivilegeAction.Full;
                    else
                        m_PageMode = Security.PrivilegeAction.View;

                    Security oSecurity = HoardersDAL.Globals.SecurityClass;
                    var lCustomerParent = (from c in DbClient.GetList<Customer>("Customer_Type=\"ParentCompany\"")
                                           join u in DbClient.GetList<User>() on c.Customer_ID equals u.Customer_ID
                                           where u.User_ID == oSecurity.UserID
                                           select new { c.Customer_Type, c.Customer_ID }).ToList();

                    HoardersDAL.User ouser = DbClient.GetList<HoardersDAL.User>().Where(u => u.User_ID == Convert.ToInt32(Session["UserID"])).SingleOrDefault<HoardersDAL.User>();
                    if (lCustomerParent.Count <= 0)
                    {
                        if (ouser.IsAdmin == true)
                        {
                            m_PageMode = Security.PrivilegeAction.Full;
                        }
                    }

                    Initialize_Controls();
                    ViewState.Add("ID", m_ID);
                    ViewState.Add("PageMode", m_PageMode);
                    Entity_Load(m_ID);
                    hdTicketNumber.Value = m_ID;
                    //akanksha 31/1/2021
                    if (m_ID == "-1")
                        lblHeaderText.Text = "Add New Tax Details";
                    else
                        lblHeaderText.Text = "Edit Tax Details";
                }
                else
                {
                    m_ID = ViewState["ID"].ToString();
                    //akanksha 31/1/2021
                    if (m_ID == "-1")
                        lblHeaderText.Text = "Add New Tax Details";
                    else
                        lblHeaderText.Text = "Edit Tax Details";
                    m_PageMode = (Security.PrivilegeAction)ViewState["PageMode"];
                    hdTicketNumber.Value = m_ID;
                }

                UpdatePanelgvTickets.Update();
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex);
            }
        }
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (m_PageMode == Security.PrivilegeAction.Full)
                {
                    if (InputIsValid() == false)
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Tax Detail : " + txtDisplayName.Text + " is already exists.');", true);
                        return;
                    }

                    Entity_AddUpdate(m_ID);
                    //ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Tax Detail Saved Successfully.');Close();", true);
                    ScriptManager.RegisterStartupScript(this, GetType(), "testfunc", "<script type='text/javascript'> alert('Tax Detail Saved Successfully.');Close(); </script>", false);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex);
            }
        }
        private void Initialize_Controls()
        {
            //FillCustomers(ddlCustomer_ID);
            ddlCustomer_ID.Items.Clear();
            List<Customer> lCustomer = DbClient.GetActiveList<Customer>().OrderBy(x => x.Customer_Name).ToList();
            ddlCustomer_ID.DataSource = lCustomer;
            ddlCustomer_ID.DataTextField = "Customer_Name";
            ddlCustomer_ID.DataValueField = "Customer_ID";
            ddlCustomer_ID.DataBind();
            ddlCustomer_ID.Items.Insert(0, new System.Web.UI.WebControls.ListItem("", "0"));
            ddlCustomer_ID.SelectedIndex = 0;

        }
        private void Entity_Load(string pkID)
        {
            DbClient oDAL = new DbClient();
            TaxDetail oRService = DbClient.GetList<TaxDetail>("ID = " + pkID).SingleOrDefault<TaxDetail>();
            HoardersDAL.User ouser = DbClient.GetList<HoardersDAL.User>().Where(u => u.User_ID == Convert.ToInt32(Session["UserID"])).SingleOrDefault<HoardersDAL.User>();

            if (oRService != null && oRService.ID.ToString() == pkID)
            {

                try
                {
                    txtDisplayName.Text = oRService.DisplayName.ToString();
                    ddlTaxType.Text = oRService.TaxType;
                    txtRate.Text = Convert.ToDecimal(oRService.TaxRate).ToString();

                    if (!string.IsNullOrEmpty(ouser.AssociateFranchise))
                    {
                        if (Globals.SecurityClass.IsCompnayAdmin != Security.UserType.Admin)
                        {
                            if (oRService.FrenchiseID == -100 || oRService.FrenchiseID == null)
                            {
                                ddlCustomer_ID.Enabled = false;
                                chkAllFranchise.Checked = true;
                                chkAllFranchise.Enabled = false;

                            }
                            else
                            {
                                BindAssociateFranchiseDropdown(ouser);
                                ddlCustomer_ID.SelectedValue = oRService.FrenchiseID.ToString();
                                ddlCustomer_ID.Enabled = true;
                                chkAllFranchise.Enabled = false;

                            }
                        }
                        else
                        {
                            if (oRService.FrenchiseID == -100 || oRService.FrenchiseID == null)
                            {
                                chkAllFranchise.Checked = true;
                                ddlCustomer_ID.Enabled = false;
                            }
                            else
                            {
                                chkAllFranchise.Enabled = false;
                                ddlCustomer_ID.SelectedValue = oRService.FrenchiseID.ToString();

                            }
                        }
                    }
                    else
                    {
                        if (Globals.SecurityClass.IsCompnayAdmin != Security.UserType.Admin)
                        {
                            if (oRService.FrenchiseID == -100 || oRService.FrenchiseID == null)
                            {
                                chkAllFranchise.Checked = true;
                                ddlCustomer_ID.Enabled = false;
                                chkAllFranchise.Enabled = false;
                            }
                            else
                            {
                                ddlCustomer_ID.SelectedValue = oRService.FrenchiseID.ToString();
                                ddlCustomer_ID.Attributes.Remove("disabled");
                                chkAllFranchise.Enabled = true;
                            }
                        }
                        else
                        {
                            if (oRService.FrenchiseID == -100 || oRService.FrenchiseID == null)
                            {
                                chkAllFranchise.Checked = true;
                                ddlCustomer_ID.Enabled = false;
                            }
                            else
                            {
                                chkAllFranchise.Enabled = false;
                                ddlCustomer_ID.SelectedValue = oRService.FrenchiseID.ToString();
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(oRService.Category))
                    {
                        ddlCategory.SelectedValue = oRService.Category;
                    }
                    if (!string.IsNullOrEmpty(oRService.State))
                    {
                        ddlState.SelectedValue = oRService.State;
                    }
                }
                catch { }
                ChkInActive.Checked = oRService.InActive == true ? true : false;

            }
            else
            {
                txtDisplayName.Text = "";
                ddlTaxType.Text = "";

                if (ddlCustomer_ID.Items.Count > 0)
                {
                    if (!string.IsNullOrEmpty(ouser.AssociateFranchise))
                    {
                        if (Globals.SecurityClass.IsCompnayAdmin != Security.UserType.Admin)
                        {
                            BindAssociateFranchiseDropdown(ouser);
                            ddlCustomer_ID.SelectedValue = Globals.SecurityClass.CurrentCompany;
                            ddlCustomer_ID.Attributes.Remove("disabled");
                            chkAllFranchise.Enabled = false;
                        }
                        else
                        {
                            ddlCustomer_ID.SelectedIndex = 0;
                            ddlCustomer_ID.Attributes.Remove("disabled");
                            chkAllFranchise.Enabled = true;
                        }
                    }
                    else
                    {
                        if (Globals.SecurityClass.IsCompnayAdmin != Security.UserType.Admin)
                        {
                            ddlCustomer_ID.SelectedValue = Globals.SecurityClass.CurrentCompany;
                            ddlCustomer_ID.Attributes.Add("disabled", "disabled");
                            chkAllFranchise.Enabled = false;
                        }
                        else
                        {
                            ddlCustomer_ID.SelectedIndex = 0;
                            ddlCustomer_ID.Attributes.Remove("disabled");
                            chkAllFranchise.Enabled = true;
                        }
                    }
                }

                ChkInActive.Checked = false;
            }

            btnSubmit.Visible = (m_PageMode == Security.PrivilegeAction.Full);
        }

        private void Entity_AddUpdate(string pkID)
        {
            DbClient oDAL = new DbClient();
            TaxDetail oRecommendedService = DbClient.GetList<TaxDetail>("ID = " + pkID).SingleOrDefault<TaxDetail>();
            if (oRecommendedService == null || oRecommendedService.ID.ToString() != pkID)
            {
                oRecommendedService = new TaxDetail();
            }
            oRecommendedService.DisplayName = txtDisplayName.Text.Trim();
            oRecommendedService.TaxType = ddlTaxType.Text;
            oRecommendedService.TaxRate = Convert.ToDecimal(txtRate.Text);
            oRecommendedService.Category = ddlCategory.SelectedValue;
            oRecommendedService.State = ddlState.SelectedValue;
            oRecommendedService.InActive = ChkInActive.Checked == true ? true : false;
            if (oRecommendedService.ID.ToString() != pkID && chkAllFranchise.Checked)
            {
                List<HoardersDAL.Customer> lstCustomer = DbClient.GetActiveList<HoardersDAL.Customer>().ToList<HoardersDAL.Customer>();
                foreach (var item in lstCustomer)
                {
                    HoardersDAL.TaxDetail td = DbClient.GetList<HoardersDAL.TaxDetail>().Where(u => u.ID != Convert.ToInt32(m_ID) && u.DisplayName == Convert.ToString(txtDisplayName.Text) && u.FrenchiseID == item.Customer_ID).FirstOrDefault<HoardersDAL.TaxDetail>();
                    if (td == null)
                    {
                        oRecommendedService.FrenchiseID = Convert.ToInt32(item.Customer_ID);
                        DbClient.Insert<TaxDetail>(oRecommendedService);
                    }
                }
            }
            else
            {
                if (chkAllFranchise.Checked)
                    oRecommendedService.FrenchiseID = -100;
                else
                    oRecommendedService.FrenchiseID = Convert.ToInt32(ddlCustomer_ID.SelectedValue);

                if (oRecommendedService.ID == 0)
                {
                    DbClient.Insert<TaxDetail>(oRecommendedService);
                }
                else
                {
                    DbClient.Update<TaxDetail>(oRecommendedService, oRecommendedService.ID);
                }
            }
        }
        protected void btnRemove_Click(object sender, EventArgs e)
        {

        }
        private bool InputIsValid()
        {
            if (!chkAllFranchise.Checked)
            {
                HoardersDAL.TaxDetail oRecommendedService = DbClient.GetList<HoardersDAL.TaxDetail>().Where(u => u.ID != Convert.ToInt32(m_ID) && u.DisplayName == Convert.ToString(txtDisplayName.Text) && u.FrenchiseID.ToString() == ddlCustomer_ID.SelectedValue).FirstOrDefault<HoardersDAL.TaxDetail>();
                if (oRecommendedService == null)
                {
                    return true;
                }
                return false;
            }
            else
                return true;
        }
        private void BindAssociateFranchiseDropdown(User ouser)
        {
            //var AssociateCustomer = (from c in DbClient.GetList<Customer>()
            //                         where c.Customer_ID == ouser.Customer_ID || c.Customer_ID.ToString().Split(',').Intersect(ouser.AssociateFranchise.Split(',')).Any()
            //                         select new { c.Customer_Name, c.Customer_ID }).ToList();
            var AssociateCustomer = (from c in DbClient.GetList<Customer>()
                                     where c.Customer_ID == ouser.Customer_ID || c.Customer_ID.ToString().Split(',').Intersect(ouser.AssociateFranchise.Split(',')).Any()
                                     select new { c.Customer_Name, c.Customer_ID }).OrderBy(x => x.Customer_Name).ToList();
            ddlCustomer_ID.Items.Clear();
            ddlCustomer_ID.DataSource = AssociateCustomer;
            ddlCustomer_ID.DataTextField = "Customer_Name";
            ddlCustomer_ID.DataValueField = "Customer_ID";
            ddlCustomer_ID.DataBind();
            ddlCustomer_ID.Enabled = true;
        }
    }
}