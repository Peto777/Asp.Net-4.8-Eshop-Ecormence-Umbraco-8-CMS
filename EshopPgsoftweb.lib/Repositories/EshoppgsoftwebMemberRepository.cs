using eshoppgsoftweb.lib.Models;
using System.Collections;
using System.Collections.Generic;
using System.Web.Security;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Repositories
{
    public class EshoppgsoftwebMemberRepository : _BaseRepository
    {
        public const string EshoppgsoftwebMemberTypeAlias = "EcommerceUser";
        public const string EshoppgsoftwebMemberAdminRole = "EcommerceAdmin";
        public const string EshoppgsoftwebMemberCustomerRole = "EcommerceCustomer";

        public List<EshoppgsoftwebMember> GetAll(string sortBy = "Name", string sortDir = "ASC")
        {
            List<EshoppgsoftwebMember> dataList = new List<EshoppgsoftwebMember>();
            EshoppgsoftwebMemberRolesInfo rolesInfo = new EshoppgsoftwebMemberRolesInfo();

            foreach (IMember member in GetAllMembers(sortBy, sortDir))
            {
                EshoppgsoftwebMember dataRec = EshoppgsoftwebMember.CreateCopyFrom(member);
                dataRec.IsAdminUser = rolesInfo.IsAdmin(this.MemberService, member);
                dataRec.IsCustomerUser = rolesInfo.IsCustomer(this.MemberService, member);
                dataList.Add(dataRec);
            }

            return dataList;
        }

        public List<EshoppgsoftwebMember> GetCustomerUsers(string sortBy = "Name", string sortDir = "ASC")
        {
            List<EshoppgsoftwebMember> dataList = new List<EshoppgsoftwebMember>();
            EshoppgsoftwebMemberRolesInfo rolesInfo = new EshoppgsoftwebMemberRolesInfo();

            foreach (IMember member in GetAllMembers(sortBy, sortDir))
            {
                EshoppgsoftwebMember dataRec = EshoppgsoftwebMember.CreateCopyFrom(member);
                if (rolesInfo.IsCustomer(this.MemberService, member))
                {
                    dataList.Add(dataRec);
                }
            }

            return dataList;
        }

        IEnumerable<IMember> GetAllMembers(string sortBy = "Name", string sortDir = "ASC")
        {
            long totalRecords;

            return this.MemberService.GetAll(0, _PagingModel.AllItemsPerPage, out totalRecords, sortBy, sortDir == "DESC" ? Direction.Descending : Direction.Ascending, EshoppgsoftwebMemberRepository.EshoppgsoftwebMemberTypeAlias);
        }

        public EshoppgsoftwebMember Get(int id)
        {
            return CreateCopyFrom(this.MemberService.GetById(id));
        }

        public EshoppgsoftwebMember Get(string id)
        {
            return Get(int.Parse(id));
        }

        public EshoppgsoftwebMember GetMemberByEmail(string email)
        {
            IMember member = this.MemberService.GetByEmail(email);
            return member == null ? null : CreateCopyFrom(member);
        }

        EshoppgsoftwebMember CreateCopyFrom(IMember imember)
        {
            EshoppgsoftwebMember member = EshoppgsoftwebMember.CreateCopyFrom(imember);

            member.IsAdminUser = System.Web.Security.Roles.IsUserInRole(member.Username, EshoppgsoftwebMemberRepository.EshoppgsoftwebMemberAdminRole);
            member.IsCustomerUser = System.Web.Security.Roles.IsUserInRole(member.Username, EshoppgsoftwebMemberRepository.EshoppgsoftwebMemberCustomerRole);

            return member;

        }
        public MembershipCreateStatus Save(PluginController ctrl, EshoppgsoftwebMember member, bool updatePermissions = true)
        {
            if (member.IsNew)
            {
                return Insert(ctrl, member);
            }
            else
            {
                return Update(member, updatePermissions);
            }
        }

        public MembershipCreateStatus Insert(PluginController ctrl, EshoppgsoftwebMember member)
        {
            if (this.MemberService.GetById(member.MemberId) != null)
            {
                return MembershipCreateStatus.DuplicateProviderUserKey;
            }

            var registerModel = ctrl.Members.CreateRegistrationModel(EshoppgsoftwebMemberRepository.EshoppgsoftwebMemberTypeAlias);
            registerModel.Name = member.Name;
            registerModel.Email = member.Email;
            registerModel.Password = member.Password;
            registerModel.Username = member.Email;
            registerModel.UsernameIsEmail = true;

            MembershipCreateStatus status;
            var newMember = ctrl.Members.RegisterMember(registerModel, out status, false);

            if (status == MembershipCreateStatus.Success)
            {
                // Assign user roles
                if (member.IsAdminUser)
                {
                    System.Web.Security.Roles.AddUserToRole(member.Username, EshoppgsoftwebMemberRepository.EshoppgsoftwebMemberAdminRole);
                }
                if (member.IsCustomerUser)
                {
                    System.Web.Security.Roles.AddUserToRole(member.Username, EshoppgsoftwebMemberRepository.EshoppgsoftwebMemberCustomerRole);
                }
            }

            return status;
            ;
        }

        public MembershipCreateStatus Update(EshoppgsoftwebMember member, bool updatePermissions)
        {
            IMember updateMember = this.MemberService.GetById(member.MemberId);
            if (updateMember == null)
            {
                return MembershipCreateStatus.UserRejected;
            }

            bool wasChange = false;

            if (updateMember.Name != member.Name)
            {
                updateMember.Name = member.Name;
                wasChange = true;
            }
            if (updateMember.Email != member.Email)
            {
                updateMember.Username = member.Email;
                updateMember.Email = member.Email;
                IMember checkMember = this.MemberService.GetByEmail(updateMember.Email);
                if (checkMember != null)
                {
                    return MembershipCreateStatus.DuplicateEmail;
                }
                wasChange = true;
            }
            if (updatePermissions)
            {
                if (updateMember.IsApproved != member.IsApproved)
                {
                    updateMember.IsApproved = member.IsApproved;
                    wasChange = true;
                }
                if (updateMember.IsLockedOut != member.IsLockedOut)
                {
                    updateMember.IsLockedOut = member.IsLockedOut;
                    wasChange = true;
                }
            }

            if (wasChange)
            {
                this.MemberService.Save(updateMember);
            }

            if (updatePermissions)
            {
                // Assign user roles
                if (member.IsAdminUser)
                {
                    System.Web.Security.Roles.AddUserToRole(member.Username, EshoppgsoftwebMemberRepository.EshoppgsoftwebMemberAdminRole);
                }
                else
                {
                    System.Web.Security.Roles.RemoveUserFromRole(member.Username, EshoppgsoftwebMemberRepository.EshoppgsoftwebMemberAdminRole);
                }
                if (member.IsCustomerUser)
                {
                    System.Web.Security.Roles.AddUserToRole(member.Username, EshoppgsoftwebMemberRepository.EshoppgsoftwebMemberCustomerRole);
                }
                else
                {
                    System.Web.Security.Roles.RemoveUserFromRole(member.Username, EshoppgsoftwebMemberRepository.EshoppgsoftwebMemberCustomerRole);
                }
            }

            return MembershipCreateStatus.Success;
        }

        public MembershipCreateStatus SavePassword(EshoppgsoftwebMember member)
        {
            IMember updateMember = this.MemberService.GetById(member.MemberId);
            if (updateMember == null)
            {
                return MembershipCreateStatus.UserRejected;
            }

            try
            {
                this.MemberService.SavePassword(updateMember, member.Password);
            }
            catch
            {
                return MembershipCreateStatus.InvalidPassword;
            }

            return MembershipCreateStatus.Success;
        }

        public MembershipCreateStatus Delete(EshoppgsoftwebMember member)
        {
            IMember deleteMember = this.MemberService.GetById(member.MemberId);
            if (deleteMember == null)
            {
                return MembershipCreateStatus.UserRejected;
            }
            this.MemberService.Delete(deleteMember);

            return MembershipCreateStatus.Success;
        }

        public string GetErrorMessage(MembershipCreateStatus status)
        {
            switch (status)
            {
                case MembershipCreateStatus.Success:
                    return string.Empty;
                case MembershipCreateStatus.DuplicateProviderUserKey:
                    return "Užívateľ už existuje";
                case MembershipCreateStatus.InvalidUserName:
                    return "Neznámy užívateľ";
                case MembershipCreateStatus.InvalidPassword:
                    return "Neplatné heslo. Zadajte heslo aspoň na 8 znakov";
                case MembershipCreateStatus.InvalidQuestion:
                    return "Nesprávna otázka";
                case MembershipCreateStatus.InvalidAnswer:
                    return "Nesprávna odpoveď";
                case MembershipCreateStatus.InvalidEmail:
                    return "Nesprávny email";
                case MembershipCreateStatus.DuplicateUserName:
                case MembershipCreateStatus.DuplicateEmail:
                    return "Užívateľ pre zadaný email už existuje";
                case MembershipCreateStatus.UserRejected:
                case MembershipCreateStatus.InvalidProviderUserKey:
                    return "Neznámy užívateľ";
                case MembershipCreateStatus.ProviderError:
                    return "Neznámy typ chyby";
            }

            return "Neznámy typ chyby";
        }
    }

    public class EshoppgsoftwebMember
    {
        public int MemberId { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordRepeat { get; set; }

        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }

        public bool IsAdminUser { get; set; }
        public bool IsCustomerUser { get; set; }

        public bool IsNew
        {
            get
            {
                return this.MemberId <= 0;
            }
        }

        public static EshoppgsoftwebMember CreateCopyFrom(IMember member)
        {
            return new EshoppgsoftwebMember()
            {
                MemberId = member.Id,
                Name = member.Name,
                Username = member.Username,
                Email = member.Email,
                IsApproved = member.IsApproved,
                IsLockedOut = member.IsLockedOut,
            };
        }
    }

    public class EshoppgsoftwebMemberRolesInfo
    {
        Hashtable htAdmin;
        Hashtable htCustomer;

        public bool IsAdmin(IMemberService service, IMember member)
        {
            return IsMemberInRole(service, member, EshoppgsoftwebMemberRepository.EshoppgsoftwebMemberAdminRole, ref htAdmin);
        }

        public bool IsCustomer(IMemberService service, IMember member)
        {
            return IsMemberInRole(service, member, EshoppgsoftwebMemberRepository.EshoppgsoftwebMemberCustomerRole, ref htCustomer);
        }

        bool IsMemberInRole(IMemberService service, IMember member, string roleName, ref Hashtable ht)
        {
            if (ht == null)
            {
                ht = LoadRolesInfo(service, roleName);
            }

            return ht.ContainsKey(member.Id);
        }

        Hashtable LoadRolesInfo(IMemberService service, string roleName)
        {
            Hashtable ht = new Hashtable();
            foreach (IMember member in service.GetMembersInRole(roleName))
            {
                ht.Add(member.Id, member);
            }

            return ht;
        }
    }
}
