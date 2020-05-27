import { Component, OnInit } from '@angular/core';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from '../roles-modal/roles-modal.component';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: User[];
  bsModalRef: BsModalRef;

  constructor(private adminService: AdminService, private modalService: BsModalService,
              private alertify: AlertifyService) { }

  ngOnInit() {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe((users: User[]) => {
        this.users = users;
    }, error => {
      this.alertify.error(error);
    });
  }

  editRolesModal(user: User) {
    console.log(user);
    const initialState = {
      user,
      roles: this.getRolesArray(user)
    };

    this.bsModalRef = this.modalService.show(RolesModalComponent, {initialState});
    this.bsModalRef.content.updateSelectedRoles.subscribe((values) => {
      const rolesToUpdate = {
        roleNames: [...values.filter(el => el.checked === true).map(el => el.name)]
      };

      // console.log(rolesToUpdate);
      if (rolesToUpdate)
      {
       this.adminService.updateUserRoles(user, rolesToUpdate).subscribe(() => {
         user.roles = [...rolesToUpdate.roleNames];
       }, error => {
         this.alertify.error(error);
       });
      }


    }, error => {
      this.alertify.error(error);
    });
    // this.bsModalRef.content.closeBtnName = 'Close';
  }

  private getRolesArray(user) {
    const roles = [];
    const userRoles = user.roles;
    const availableRoles: any[] = [
      {name: 'Admin', value: 'Admin'},
      {name: 'Moderator', value: 'Moderator'},
      {name: 'Member', value: 'Member'},
      {name: 'VIP', value: 'VIP'}
    ];

    for (let i = 0; i < availableRoles.length; i++)
    {
      let isMatch = false;

      // See if it's checked
      for (let j = 0; j < userRoles.length; j++)
      {
        if (availableRoles[i].name === userRoles[j])
        {
          isMatch = true;
          availableRoles[i].checked = true;
          roles.push(availableRoles[i]);
          break;
        }
      }

      if (!isMatch)
      {
        availableRoles[i].checked = false;
        roles.push(availableRoles[i]);
      }
    }

    return roles;
  }
}
