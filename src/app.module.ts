import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { AuthModule } from './auth/auth.module';
import { UsersModule } from './users/users.module';
import { PrismaModule } from './prisma/prisma.module';
import { BillsService } from './bills/bills.service';
import { BillsModule } from './bills/bills.module';

@Module({
  imports: [PrismaModule, AuthModule, UsersModule, BillsModule],
  controllers: [AppController],
  providers: [AppService, BillsService],
})
export class AppModule {}
